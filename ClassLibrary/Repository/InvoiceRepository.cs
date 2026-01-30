using Library.Data;
using Library.Models.Invoice;
using Library.Models.Order;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Library.Repository
{
    public class InvoiceRepository
    {
        private readonly string _cs = SqlHelper.connStr;

        // ORDERS LIST
        public List<InvoiceHeaderModel> GetInvoices()
        {
            var list = new List<InvoiceHeaderModel>();

            DataTable dt = SqlHelper.Select(@"
                SELECT TOP 1000
                    I.LOGICALREF,
                    I.FICHENO,
                    I.DATE_,
                    I.TIME_,
                    I.CLIENTREF,
                    I.GRPCODE,
                    C.DEFINITION_ AS CLIENTNAME,
                    I.NETTOTAL
                FROM LG_001_01_INVOICE I
                LEFT JOIN LG_001_CLCARD C ON C.LOGICALREF = I.CLIENTREF
                ORDER BY I.LOGICALREF DESC
            ");

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new InvoiceHeaderModel
                {
                
                    LOGICALREF = row["LOGICALREF"] == DBNull.Value ? 0 :
                        Convert.ToInt32(row["LOGICALREF"]),
                    FICHENO = row["FICHENO"]?.ToString() ?? "",
                    DATE_ = row["DATE_"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(row["DATE_"]),
                    TIME_ = row["TIME_"] == DBNull.Value ? 0 :
                        Convert.ToInt32(row["TIME_"]),
                    CLIENTREF = row["CLIENTREF"] == DBNull.Value ? 0 :
                        Convert.ToInt32(row["CLIENTREF"]),
                    CLIENTNAME = row["CLIENTNAME"]?.ToString() ?? "",
                    InvoiceType = row["GRPCODE"] == DBNull.Value ? 0 : 
                    Convert.ToInt32(row["GRPCODE"]),
                    NETTOTAL = row["NETTOTAL"] == DBNull.Value ? 0 :
                        Convert.ToDecimal(row["NETTOTAL"])

                });
            }

            return list;
        }

        public bool DeleteInvoice(int logicalRef)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();

            using SqlTransaction tran = con.BeginTransaction();
            try
            {
                SqlHelper.Execute(
                    "DELETE FROM LG_001_01_STLINE WHERE INVOICEREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", logicalRef));

                int rows = SqlHelper.Execute(
                    "DELETE FROM LG_001_01_INVOICE WHERE LOGICALREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", logicalRef));

                tran.Commit();
                return rows > 0;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // CLIENTS
        public List<SelectListItem> GetClients()
        {
            var list = new List<SelectListItem>();

            DataTable dt = SqlHelper.Select(@"
                SELECT LOGICALREF, DEFINITION_
                FROM LG_001_CLCARD
                ORDER BY LOGICALREF DESC
            ");

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["LOGICALREF"].ToString(),
                    Text = row["DEFINITION_"].ToString()
                });
            }

            return list;
        }

        // ITEMS
        public List<InvoiceItemViewModel> GetItems()
        {
            var list = new List<InvoiceItemViewModel>();

            DataTable dt = SqlHelper.Select(@"
                SELECT
                    i.LOGICALREF,
                    i.CODE AS MalzemeKodu,
                    i.NAME AS MalzemeAdi,
                    g.ONHAND AS FiiliStok
                FROM LG_001_ITEMS i
                JOIN LV_001_01_GNTOTST g ON i.LOGICALREF = g.STOCKREF
                WHERE g.INVENNO = 0
                ORDER BY i.LOGICALREF
            ");

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new InvoiceItemViewModel
                {
                    Value = row["LOGICALREF"].ToString() ?? "",
                    Code = row["MalzemeKodu"].ToString() ?? "",
                    Text = row["MalzemeAdi"].ToString() ?? "",
                    OnHand = Convert.ToDecimal(row["FiiliStok"])
                });
            }
            return list;
        }

        //Prices
        public decimal GetItemPrice(int itemRef, int uomRef, int invoiceType)
        {
            // invoiceType:
            // 1 = Alış → PTYPE = 1
            // 2 = Satış → PTYPE = 2
            int ptype = invoiceType;

            object result = SqlHelper.ExecuteScalar(@"
                    SELECT TOP 1 PRICE
                    FROM LG_001_PRCLIST
                    WHERE CARDREF = @itemRef
                      AND UOMREF  = @uomRef
                      AND PTYPE   = @ptype
                      AND ACTIVE  = 0
                    ORDER BY LOGICALREF DESC",
                new SqlParameter("@itemRef", itemRef),
                new SqlParameter("@uomRef", uomRef),
                new SqlParameter("@ptype", ptype)
            );
           
            return result == null || result == DBNull.Value
                ? 0
                : Convert.ToDecimal(result);

        }



        // UNITS
        public Dictionary<int, List<SelectListItem>> GetUnits()
        {
            var dict = new Dictionary<int, List<SelectListItem>>();

            DataTable dt = SqlHelper.Select(@"
                SELECT 
                    IU.ITEMREF,
                    IU.UNITLINEREF AS UOMREF,
                    UL.UNITSETREF AS USREF,
                    UL.NAME
                FROM LG_001_ITMUNITA IU
                JOIN LG_001_UNITSETL UL ON UL.LOGICALREF = IU.UNITLINEREF
                ORDER BY IU.ITEMREF, IU.LINENR
            ");

            foreach (DataRow row in dt.Rows)
            {
                int itemRef = Convert.ToInt32(row["ITEMREF"]);

                if (!dict.ContainsKey(itemRef))
                    dict[itemRef] = new List<SelectListItem>();

                dict[itemRef].Add(new SelectListItem
                {
                    Value = $"{row["UOMREF"]}|{row["USREF"]}",
                    Text = row["NAME"].ToString()
                });
            }

            return dict;
        }

        // CREATE INVOICE
        public void CreateInvoice(InvoiceCreateViewModel model)
        {
            using SqlConnection con = new SqlConnection(_cs);
            con.Open();
            using SqlTransaction tran = con.BeginTransaction();
            if (model.Header.GRPCODE == 1)
            {
                model.Header.TRCODE = 1; // Satın Alma Faturası
            }
            if (model.Header.GRPCODE == 2)
            {
                model.Header.TRCODE = 8; // Toptan satış faturası
            }

            try
            {
                int invoiceRef = Convert.ToInt32(SqlHelper.Scalar(@"
                    INSERT INTO LG_001_01_INVOICE
                    (GRPCODE, TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
                     ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED, TOTALVAT, GROSSTOTAL,
                     NETTOTAL,REPORTRATE, REPORTNET,
                     BRANCH, DEPARTMENT ,RECSTATUS, STATUS, CANCELLED
                    )
                    OUTPUT INSERTED.LOGICALREF
                    VALUES
                    (@GRPCODE, @TRCODE, @FICHENO, @DATE_, @TIME_, @CLIENTREF,
                     @ADDDISCOUNTS, @TOTALDISCOUNTS, @TOTALDISCOUNTED,@TOTALVAT,@GROSSTOTAL,
                     @NETTOTAL,1, @REPORTNET,
                     0, 0, 1, 0, 0)
                ", con, tran,

                
                new SqlParameter("@GRPCODE", model.Header.GRPCODE),
                new SqlParameter("@TRCODE", model.Header.TRCODE),
                new SqlParameter("@FICHENO", (object?)model.Header.FICHENO ?? DBNull.Value),
                new SqlParameter("@DATE_", model.Header.DATE_),
                new SqlParameter("@TIME_", model.Header.TIME_),
                new SqlParameter("@CLIENTREF", model.Header.CLIENTREF),
                new SqlParameter("@ADDDISCOUNTS", model.Header.ADDDISCOUNTS),
                new SqlParameter("@TOTALDISCOUNTS", model.Header.TOTALDISCOUNTS),
                new SqlParameter("@TOTALDISCOUNTED", model.Header.TOTALDISCOUNTED),
                new SqlParameter("@GROSSTOTAL", model.Header.GROSSTOTAL),
                new SqlParameter("@TOTALVAT", model.Header.TOTALVAT),
                new SqlParameter("@NETTOTAL", model.Header.NETTOTAL),
                new SqlParameter("@REPORTNET", model.Header.REPORTNET)
                ));

                int lineNo = 1;
                foreach (var line in model.Lines)
                {
                    decimal total = line.AMOUNT * line.PRICE;
                    decimal vatAmount = total * line.VAT / 100;

                    SqlHelper.Execute(@"
                        INSERT INTO LG_001_01_STLINE
                        (
                         INVOICEREF, INVOICELNNO, STOCKREF, CLIENTREF,
                         LINETYPE, DETLINE, TRCODE, DATE_, FTIME, AMOUNT, PRICE, TOTAL, 
                         VAT, VATAMNT, VATMATRAH,  LINENET, UOMREF, USREF,
                         UINFO1, UINFO2, GROSSUINFO1, GROSSUINFO2,
                         DORESERVE, RECSTATUS, CANCELLED, STATUS
                        )
                        VALUES
                        (@INVOICEREF, @INVOICELNNO, @STOCKREF, @CLIENTREF, 
                        0, 0, @TRCODE, @DATE_, @FTIME,@AMOUNT, @PRICE, @TOTAL,
                        @VAT, @VATAMNT, @VATMATRAH, @LINENET, @UOMREF, @USREF,
                        1, 1, @GROSSUINFO1, 1,
                        0, 1, 0, 0)
                    ", con, tran,
                    new SqlParameter("@INVOICEREF", invoiceRef),
                    new SqlParameter("@INVOICELNNO", lineNo++),
                    new SqlParameter("@STOCKREF", line.STOCKREF),
                    new SqlParameter("@CLIENTREF", model.Header.CLIENTREF),
                    new SqlParameter("@TRCODE", model.Header.TRCODE),
                    new SqlParameter("@DATE_", model.Header.DATE_),
                    new SqlParameter("FTIME", model.Header.TIME_),
                    new SqlParameter("@AMOUNT", line.AMOUNT),
                    new SqlParameter("@PRICE", line.PRICE),
                    new SqlParameter("@TOTAL", total),
                    new SqlParameter("@VAT", line.VAT),
                    new SqlParameter("@VATAMNT", vatAmount),
                    new SqlParameter("@VATMATRAH", total),
                    new SqlParameter("@LINENET", total),
                    new SqlParameter("@UOMREF", line.UOMREF),
                    new SqlParameter("@USREF", line.USREF),
                    new SqlParameter("@GROSSUINFO1", line.AMOUNT)
                    );
                }
                // Indirim satiri
                if (model.Header.TOTALDISCOUNTS > 0)
                {
                    SqlHelper.Execute(@"
                INSERT INTO LG_001_01_STLINE
                (INVOICEREF, STOCKREF, CLIENTREF, LINETYPE, INVOICELNNO, DETLINE,
                  TRCODE, DATE_, FTIME, TOTAL,
                 RECSTATUS, CANCELLED)
                VALUES
                (@invoiceRef, 0, @CLIENTREF, 2, @INVOICELNNO, 0,
                 @TRCODE, @DATE_, @FTIME, @TOTAL,
                 1, 0)",
                        con, tran,
                        new SqlParameter("@invoiceRef", invoiceRef),
                        new SqlParameter("@CLIENTREF", model.Header.CLIENTREF),
                        new SqlParameter("@INVOICELNNO", lineNo),
                        new SqlParameter("@TRCODE", model.Header.TRCODE),
                        new SqlParameter("@DATE_", model.Header.DATE_),
                        new SqlParameter("@FTIME", model.Header.TIME_),
                        new SqlParameter("@TOTAL", model.Header.TOTALDISCOUNTS)
                    );
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public InvoiceCreateViewModel? GetInvoiceForEdit(int invoiceRef)
        {
            var model = new InvoiceCreateViewModel();

            // ================= HEADER =================
            DataTable dtHeader = SqlHelper.Select(@"
                SELECT  LOGICALREF, GRPCODE, TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
                    GROSSTOTAL, TOTALVAT, NETTOTAL,
                    ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED, REPORTNET
                    FROM LG_001_01_INVOICE
                    WHERE LOGICALREF = @ID",
                new SqlParameter("@ID", invoiceRef));

            if (dtHeader.Rows.Count == 0)
                return null;

            DataRow h = dtHeader.Rows[0];

            model.Header = new InvoiceHeaderModel
            {
                LOGICALREF = Convert.ToInt32(h["LOGICALREF"]),
                GRPCODE = Convert.ToInt16(h["GRPCODE"]),
                TRCODE = Convert.ToInt16(h["TRCODE"]),
                FICHENO = h["FICHENO"]?.ToString() ?? string.Empty,
                DATE_ = Convert.ToDateTime(h["DATE_"]),
                TIME_ = Convert.ToInt32(h["TIME_"]),
                CLIENTREF = Convert.ToInt32(h["CLIENTREF"]),
                GROSSTOTAL = Convert.ToDecimal(h["GROSSTOTAL"]),
                TOTALVAT = Convert.ToDecimal(h["TOTALVAT"]),
                NETTOTAL = Convert.ToDecimal(h["NETTOTAL"]),
                ADDDISCOUNTS = Convert.ToDecimal(h["ADDDISCOUNTS"]),
                TOTALDISCOUNTS = Convert.ToDecimal(h["TOTALDISCOUNTS"]),
                TOTALDISCOUNTED = Convert.ToDecimal(h["TOTALDISCOUNTED"]),
                REPORTNET = Convert.ToDecimal(h["REPORTNET"])
            };

            // ================= LINES =================
            model.Lines = new List<InvoiceLineModel>();

            DataTable dtLines = SqlHelper.Select(@"
                SELECT 
                    STOCKREF, AMOUNT, PRICE, VAT,
                    UOMREF, USREF,
                    LINENET, VATAMNT
                FROM LG_001_01_STLINE
                WHERE INVOICEREF = @ID
                  AND LINETYPE = 0
                ORDER BY INVOICELNNO",
                new SqlParameter("@ID", invoiceRef));

            foreach (DataRow l in dtLines.Rows)
            {
                model.Lines.Add(new InvoiceLineModel
                {
                    STOCKREF = Convert.ToInt32(l["STOCKREF"]),
                    AMOUNT = Convert.ToDecimal(l["AMOUNT"]),
                    PRICE = Convert.ToDecimal(l["PRICE"]),
                    VAT = Convert.ToInt32(l["VAT"]),
                    VATAMNT = Convert.ToDecimal(l["VATAMNT"]),
                    LINENET = Convert.ToDecimal(l["LINENET"]),
                    UOMREF = Convert.ToInt32(l["UOMREF"]),
                    USREF = Convert.ToInt32(l["USREF"])
                });
            }

            return model;
        }

       
        public void UpdateInvoiceHeader(InvoiceHeaderModel header)
        {
           

            int rows = SqlHelper.Execute(@"
                UPDATE LG_001_01_INVOICE
                SET 
                    GRPCODE = @GRPCODE,
                    TRCODE = @TRCODE,
                    FICHENO = @FICHENO,
                    DATE_ = @DATE_,
                    TIME_ = @TIME_,
                    CLIENTREF = @CLIENTREF,
                    NETTOTAL = @NETTOTAL,
                    GROSSTOTAL = @GROSSTOTAL,
                    TOTALVAT = @TOTALVAT,
                    TOTALDISCOUNTS = @TOTALDISCOUNTS,
                    TOTALDISCOUNTED = @TOTALDISCOUNTED,
                    ADDDISCOUNTS = @ADDDISCOUNTS,
                    REPORTNET = @REPORTNET
                WHERE LOGICALREF = @LOGICALREF",
                
                new SqlParameter("@LOGICALREF", header.LOGICALREF),
                new SqlParameter("@DATE_", header.DATE_),
                new SqlParameter("@TIME_", header.TIME_),
                new SqlParameter("@CLIENTREF", header.CLIENTREF),
                new SqlParameter("@GRPCODE", header.GRPCODE),
                new SqlParameter("@TRCODE", header.TRCODE),
                new SqlParameter("@NETTOTAL", header.NETTOTAL),
                new SqlParameter("@GROSSTOTAL", header.GROSSTOTAL),
                new SqlParameter("@TOTALVAT", header.TOTALVAT),
                new SqlParameter("@TOTALDISCOUNTS", header.TOTALDISCOUNTS),
                new SqlParameter("@TOTALDISCOUNTED", header.TOTALDISCOUNTED),
                new SqlParameter("@ADDDISCOUNTS", header.ADDDISCOUNTS),
                new SqlParameter("@REPORTNET", header.REPORTNET),
                new SqlParameter("@FICHENO", header.FICHENO)
            );

            if (rows == 0)
                throw new Exception("Header güncellenemedi. LOGICALREF yanlış olabilir.");
        }


        public void UpdateInvoiceLines(int headerRef, List<InvoiceLineModel> lines, InvoiceHeaderModel header)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();

            using SqlTransaction tran = con.BeginTransaction();
            try
            {
                //Eski satırları sil
                SqlHelper.Execute(
                    "DELETE FROM LG_001_01_STLINE WHERE INVOICEREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", headerRef));

                //Yeni satırları ekle
                int lineNo = 1;
                foreach (var line in lines)
                {
                    decimal total = line.AMOUNT * line.PRICE;
                    decimal vatAmount = total * line.VAT / 100;

                    SqlHelper.Execute(@"
                        INSERT INTO LG_001_01_STLINE
                        (INVOICEREF, INVOICELNNO, STOCKREF, CLIENTREF,
                         LINETYPE, DETLINE, TRCODE, DATE_, FTIME, AMOUNT, PRICE, TOTAL, 
                         VAT, VATAMNT, VATMATRAH,  LINENET, UOMREF, USREF,
                         UINFO1, UINFO2, GROSSUINFO1, GROSSUINFO2,
                         DORESERVE, RECSTATUS, CANCELLED, STATUS)
                        VALUES
                        (@invoiceRef, @INVOICELNNO, @STOCKREF, @CLIENTREF, 
                        0, 0, @TRCODE, @DATE_, @FTIME, @AMOUNT, @PRICE, @TOTAL,
                        @VAT, @VATAMNT, @VATMATRAH, @LINENET, @UOMREF, @USREF,
                        1, 1, @GROSSUINFO1, 1,
                        0, 1, 0, 0)",
                    con, tran,

                    new SqlParameter("@invoiceRef", headerRef),
                    new SqlParameter("@STOCKREF", line.STOCKREF),
                    new SqlParameter("@CLIENTREF", header.CLIENTREF),
                    new SqlParameter("@INVOICELNNO", lineNo++),
                    new SqlParameter("@TRCODE", header.TRCODE),
                    new SqlParameter("@DATE_", header.DATE_),
                    new SqlParameter("@FTIME", header.TIME_),
                    new SqlParameter("@AMOUNT", line.AMOUNT),
                    new SqlParameter("@PRICE", line.PRICE),
                    new SqlParameter("@TOTAL", total),
                    new SqlParameter("@VAT", line.VAT),
                    new SqlParameter("@VATAMNT", vatAmount),
                    new SqlParameter("@VATMATRAH", total),
                    new SqlParameter("@LINENET", total),
                    new SqlParameter("@UOMREF", line.UOMREF),
                    new SqlParameter("@USREF", line.USREF),
                    new SqlParameter("@GROSSUINFO1", line.AMOUNT),
                    new SqlParameter("@GROSSUINFO2", 1),
                    new SqlParameter("@SHIPPEDAMOUNT", line.AMOUNT)
                    );
                }

                //İndirim satırı
                if (header.TOTALDISCOUNTS > 0)
                {
                    SqlHelper.Execute(@"
                INSERT INTO LG_001_01_STLINE
                (INVOICEREF, STOCKREF, CLIENTREF, LINETYPE, INVOICELNNO, DETLINE,
                TRCODE, DATE_, FTIME, TOTAL,
                 RECSTATUS, CANCELLED)
                VALUES
                (@invoiceRef, 0, @CLIENTREF, 2, @INVOICELNNO, 0,
                 @TRCODE, @DATE_, @FTIME, @TOTAL,
                 1, 0)",
                        con, tran,
                        new SqlParameter("@invoiceRef", headerRef),
                        new SqlParameter("@CLIENTREF", header.CLIENTREF),
                        new SqlParameter("@INVOICELNNO", lineNo),
                        new SqlParameter("@TRCODE", header.TRCODE),
                        new SqlParameter("@DATE_", header.DATE_),
                        new SqlParameter("@FTIME", header.TIME_),
                        new SqlParameter("@TOTAL", header.TOTALDISCOUNTS)
                    );
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }







    }
}

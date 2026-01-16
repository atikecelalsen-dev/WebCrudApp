
using Library.Data;
using Library.Models.Order;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.PortableExecutable;

namespace Library.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _cs = SqlHelper.connStr;

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
        public List<OrderItemViewModel> GetItems()
        {
            var list = new List<OrderItemViewModel>();

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
                list.Add(new OrderItemViewModel
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
        public decimal GetItemPrice(int itemRef, int uomRef, int orderType)
        {
            // orderType:
            // 1 = Satış → PTYPE = 2
            // 2 = Alış  → PTYPE = 1
            int ptype = orderType == 1 ? 2 : 1;

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

        // ORDERS LIST
        public List<OrderHeaderModel> GetOrders()
        {
            var list = new List<OrderHeaderModel>();

            DataTable dt = SqlHelper.Select(@"
                SELECT TOP 1000
                    O.LOGICALREF,
                    O.FICHENO,
                    O.TRCODE,
                    O.DATE_,
                    O.TIME_,
                    O.CLIENTREF,
                    C.DEFINITION_ AS CLIENTNAME,
                    O.NETTOTAL
                FROM LG_001_01_ORFICHE O
                LEFT JOIN LG_001_CLCARD C ON C.LOGICALREF = O.CLIENTREF
                ORDER BY O.LOGICALREF DESC
            ");

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new OrderHeaderModel
                {
                    LOGICALREF = Convert.ToInt32(row["LOGICALREF"]),
                    FICHENO = row["FICHENO"].ToString(),
                    OrderType = Convert.ToInt32(row["TRCODE"]),
                    DATE_ = Convert.ToDateTime(row["DATE_"]),
                    TIME_ = Convert.ToInt32(row["TIME_"]),
                    CLIENTREF = Convert.ToInt32(row["CLIENTREF"]),
                    CLIENTNAME = row["CLIENTNAME"].ToString(),
                    NETTOTAL = Convert.ToDecimal(row["NETTOTAL"])
                });
            }

            return list;
        }

        // CREATE ORDER
        public void CreateOrder(OrderCreateViewModel model)
        {
            using SqlConnection con = new SqlConnection(_cs);
            con.Open();
            using SqlTransaction tran = con.BeginTransaction();

            try
            {
                int ordFicheRef = Convert.ToInt32(SqlHelper.Scalar(@"
                    INSERT INTO LG_001_01_ORFICHE
                    (TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
                     GROSSTOTAL, TOTALVAT, NETTOTAL,
                     REPORTNET, ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED,
                     BRANCH, DEPARTMENT, STATUS, RECSTATUS, CANCELLED)
                    OUTPUT INSERTED.LOGICALREF
                    VALUES
                    (@TRCODE, @FICHENO, @DATE_, @TIME_, @CLIENTREF,
                     @GROSSTOTAL, @TOTALVAT, @NETTOTAL,
                     @REPORTNET, @ADDDISCOUNTS, @TOTALDISCOUNTS, @TOTALDISCOUNTED,
                     0, 0, 1, 1, 0)
                ", con, tran,
                new SqlParameter("@TRCODE", model.Header.TRCODE),
                new SqlParameter("@FICHENO", (object?)model.Header.FICHENO ?? DBNull.Value),
                new SqlParameter("@DATE_", model.Header.DATE_),
                new SqlParameter("@TIME_", model.Header.TIME_),
                new SqlParameter("@CLIENTREF", model.Header.CLIENTREF),
                new SqlParameter("@GROSSTOTAL", model.Header.GROSSTOTAL),
                new SqlParameter("@TOTALVAT", model.Header.TOTALVAT),
                new SqlParameter("@NETTOTAL", model.Header.NETTOTAL),
                new SqlParameter("@REPORTNET", model.Header.REPORTNET),
                new SqlParameter("@ADDDISCOUNTS", model.Header.ADDDISCOUNTS),
                new SqlParameter("@TOTALDISCOUNTS", model.Header.TOTALDISCOUNTS),
                new SqlParameter("@TOTALDISCOUNTED", model.Header.TOTALDISCOUNTED)
                ));

                int lineNo = 1;
                foreach (var line in model.Lines)
                {
                    decimal total = line.AMOUNT * line.PRICE;
                    decimal vatAmount = total * line.VAT / 100;

                    SqlHelper.Execute(@"
                        INSERT INTO LG_001_01_ORFLINE
                        (ORDFICHEREF, STOCKREF, CLIENTREF, LINETYPE, LINENO_,
                         TRCODE, DATE_, TIME_,
                         AMOUNT, PRICE, TOTAL,
                         VAT, VATAMNT, VATMATRAH, LINENET,
                         UOMREF, USREF,
                         RECSTATUS, CANCELLED,
                         UINFO1, UINFO2, GROSSUINFO1, GROSSUINFO2,
                         SHIPPEDAMOUNT, CLOSED, DORESERVE)
                        VALUES
                        (@ORDFICHEREF, @STOCKREF, @CLIENTREF, 0, @LINENO_,
                         @TRCODE, @DATE_, @TIME_,
                         @AMOUNT, @PRICE, @TOTAL,
                         @VAT, @VATAMNT, @VATMATRAH, @LINENET,
                         @UOMREF, @USREF,
                         1, 0,
                         1, 1, @GROSSUINFO1, 1,
                         @SHIPPEDAMOUNT, 0, 0)
                    ", con, tran,
                    new SqlParameter("@ORDFICHEREF", ordFicheRef),
                    new SqlParameter("@STOCKREF", line.STOCKREF),
                    new SqlParameter("@CLIENTREF", model.Header.CLIENTREF),
                    new SqlParameter("@LINENO_", lineNo++),
                    new SqlParameter("@TRCODE", model.Header.TRCODE),
                    new SqlParameter("@DATE_", model.Header.DATE_),
                    new SqlParameter("@TIME_", model.Header.TIME_),
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
                    new SqlParameter("@SHIPPEDAMOUNT", line.AMOUNT)
                    );
                }
                //İndirim satırı
                if (model.Header.TOTALDISCOUNTS > 0)
                {
                    SqlHelper.Execute(@"
                INSERT INTO LG_001_01_ORFLINE
                (ORDFICHEREF, STOCKREF, CLIENTREF, LINETYPE, LINENO_, DETLINE,
                TRCODE, DATE_, TIME_, TOTAL,
                 RECSTATUS, CANCELLED)
                VALUES
                (@ORDFICHEREF, 0, @CLIENTREF, 2, @LINENO_, 0,
                 @TRCODE, @DATE_, @TIME_, @TOTAL,
                 1, 0)",
                        con, tran,
                        new SqlParameter("@ORDFICHEREF", ordFicheRef),
                        new SqlParameter("@CLIENTREF", model.Header.CLIENTREF),
                        new SqlParameter("@LINENO_", lineNo),
                        new SqlParameter("@TRCODE", model.Header.TRCODE),
                        new SqlParameter("@DATE_", model.Header.DATE_),
                        new SqlParameter("@TIME_", model.Header.TIME_),
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
   

        public OrderCreateViewModel? GetOrderForEdit(int ordFicheRef)
        {
            var model = new OrderCreateViewModel();

            // ================= HEADER =================
            DataTable dtHeader = SqlHelper.Select(@"
                SELECT  LOGICALREF, TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
                    GROSSTOTAL, TOTALVAT, NETTOTAL,
                    ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED, REPORTNET
                    FROM LG_001_01_ORFICHE
                    WHERE LOGICALREF = @ID",
                new SqlParameter("@ID", ordFicheRef));

            if (dtHeader.Rows.Count == 0)
                return null;

            DataRow h = dtHeader.Rows[0];

            model.Header = new OrderHeaderModel
            {
                LOGICALREF = Convert.ToInt32(h["LOGICALREF"]),
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
            model.Lines = new List<OrderLineModel>();

            DataTable dtLines = SqlHelper.Select(@"
                SELECT 
                    STOCKREF, AMOUNT, PRICE, VAT,
                    UOMREF, USREF,
                    LINENET, VATAMNT
                FROM LG_001_01_ORFLINE
                WHERE ORDFICHEREF = @ID
                  AND LINETYPE = 0
                ORDER BY LINENO_",
                new SqlParameter("@ID", ordFicheRef));

            foreach (DataRow l in dtLines.Rows)
            {
                model.Lines.Add(new OrderLineModel
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

        public bool DeleteOrder(int logicalRef)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();

            using SqlTransaction tran = con.BeginTransaction();
            try
            {
                SqlHelper.Execute(
                    "DELETE FROM LG_001_01_ORFLINE WHERE ORDFICHEREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", logicalRef));

                int rows = SqlHelper.Execute(
                    "DELETE FROM LG_001_01_ORFICHE WHERE LOGICALREF = @REF",
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

        public void UpdateOrderHeader(OrderHeaderModel header)
        {
            int rows = SqlHelper.Execute(@"
                UPDATE LG_001_01_ORFICHE
                SET 
                    DATE_ = @DATE_,
                    TIME_ = @TIME_,
                    CLIENTREF = @CLIENTREF,
                    TRCODE = @TRCODE,
                    NETTOTAL = @NETTOTAL,
                    GROSSTOTAL = @GROSSTOTAL,
                    TOTALVAT = @TOTALVAT,
                    TOTALDISCOUNTS = @TOTALDISCOUNTS,
                    TOTALDISCOUNTED = @TOTALDISCOUNTED,
                    ADDDISCOUNTS = @ADDDISCOUNTS,
                    REPORTNET = @REPORTNET,
                    FICHENO = @FICHENO
                WHERE LOGICALREF = @LOGICALREF",
                new SqlParameter("@LOGICALREF", header.LOGICALREF),
                new SqlParameter("@DATE_", header.DATE_),
                new SqlParameter("@TIME_", header.TIME_),
                new SqlParameter("@CLIENTREF", header.CLIENTREF),
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


        public void UpdateOrderLines(int headerRef, List<OrderLineModel> lines, OrderHeaderModel header)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();

            using SqlTransaction tran = con.BeginTransaction();
            try
            {
                //Eski satırları sil
                SqlHelper.Execute(
                    "DELETE FROM LG_001_01_ORFLINE WHERE ORDFICHEREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", headerRef));

                //Yeni satırları ekle
                int lineNo = 1;
                foreach (var line in lines)
                {
                    decimal total = line.AMOUNT * line.PRICE;
                    decimal vatAmount = total * line.VAT / 100;

                    SqlHelper.Execute(@"
                        INSERT INTO LG_001_01_ORFLINE
                        (ORDFICHEREF, STOCKREF, CLIENTREF, LINETYPE, LINENO_, DETLINE,
                         TRCODE, DATE_, TIME_, AMOUNT, PRICE, TOTAL,
                         VAT, VATAMNT, VATMATRAH, LINENET, UOMREF, USREF,
                         RECSTATUS, CANCELLED, UINFO1, UINFO2, 
                         GROSSUINFO1, GROSSUINFO2, SHIPPEDAMOUNT, CLOSED, DORESERVE)
                        VALUES
                        (@ORDFICHEREF, @STOCKREF, @CLIENTREF, 0, @LINENO_, 0,
                         @TRCODE, @DATE_, @TIME_, @AMOUNT, @PRICE, @TOTAL,
                         @VAT, @VATAMNT, @VATMATRAH, @LINENET, @UOMREF, @USREF,
                         1, 0, 1, 1,
                         @GROSSUINFO1, @GROSSUINFO2, @SHIPPEDAMOUNT, 0, 0)",
                    con, tran,
                    new SqlParameter("@ORDFICHEREF", headerRef),
                    new SqlParameter("@STOCKREF", line.STOCKREF),
                    new SqlParameter("@CLIENTREF", header.CLIENTREF),
                    new SqlParameter("@LINENO_", lineNo++),
                    new SqlParameter("@TRCODE", header.TRCODE),
                    new SqlParameter("@DATE_", header.DATE_),
                    new SqlParameter("@TIME_", header.TIME_),
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
                INSERT INTO LG_001_01_ORFLINE
                (ORDFICHEREF, STOCKREF, CLIENTREF, LINETYPE, LINENO_, DETLINE,
                 TRCODE, DATE_, TIME_, TOTAL,
                 RECSTATUS, CANCELLED)
                VALUES
                (@ORDFICHEREF, 0, @CLIENTREF, 2, @LINENO_, 0,
                 @TRCODE, @DATE_, @TIME_, @TOTAL,
                 1, 0)",
                        con, tran,
                        new SqlParameter("@ORDFICHEREF", headerRef),
                        new SqlParameter("@CLIENTREF", header.CLIENTREF),
                        new SqlParameter("@LINENO_", lineNo),
                        new SqlParameter("@TRCODE", header.TRCODE),
                        new SqlParameter("@DATE_", header.DATE_),
                        new SqlParameter("@TIME_", header.TIME_),
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

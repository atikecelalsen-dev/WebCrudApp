using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using WebCrudApp.Models;

public class OrderRepository : IOrderRepository
{
    private readonly string _cs =
        "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;TrustServerCertificate=True;";

    // Müşteri dropdown
    public List<SelectListItem> GetClients()
    {
        var list = new List<SelectListItem>();
        using var con = new SqlConnection(_cs);
        con.Open();
        using var cmd = new SqlCommand(@"
                SELECT LOGICALREF, DEFINITION_
                FROM LG_001_CLCARD
                ORDER BY LOGICALREF
            ", con);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            list.Add(new SelectListItem
            {
                Value = dr["LOGICALREF"].ToString(),
                Text = dr["DEFINITION_"].ToString()
            });
        }
        return list;
    }

    // Ürün dropdown
    public List<SelectListItem> GetItems()
    {
        var list = new List<SelectListItem>();
        using var con = new SqlConnection(_cs);
        con.Open();
        using var cmd = new SqlCommand(@"
                SELECT LOGICALREF, NAME
                FROM LG_001_ITEMS
                ORDER BY LOGICALREF
            ", con);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            list.Add(new SelectListItem
            {
                Value = dr["LOGICALREF"].ToString(),
                Text = dr["NAME"].ToString()
            });
        }
        return list;
    }

    // Ürün birimleri dropdown (UOM + USREF)
    public Dictionary<int, List<SelectListItem>> GetUnits()
    {
        var dict = new Dictionary<int, List<SelectListItem>>();
        using var con = new SqlConnection(_cs);
        con.Open();
        using var cmd = new SqlCommand(@"
                SELECT 
                    IU.ITEMREF,
                    IU.UNITLINEREF AS UOMREF,
                    UL.UNITSETREF AS USREF,   
                    UL.NAME
                FROM LG_001_ITMUNITA IU
                JOIN LG_001_UNITSETL UL ON UL.LOGICALREF = IU.UNITLINEREF 
                ORDER BY IU.ITEMREF, IU.LINENR
            ", con);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            int itemRef = Convert.ToInt32(dr["ITEMREF"]);
            if (!dict.ContainsKey(itemRef))
                dict[itemRef] = new List<SelectListItem>();

            dict[itemRef].Add(new SelectListItem
            {
                Value = $"{dr["UOMREF"]}|{dr["USREF"]}",
                Text = dr["NAME"].ToString()
            });
        }
        return dict;
    }


    public List<OrderHeaderModel> GetOrders()
    {
        var list = new List<OrderHeaderModel>();

        using SqlConnection con = new SqlConnection(_cs);
        con.Open();

        SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 1000 LOGICALREF, FICHENO, DATE_, TIME_, CLIENTREF, NETTOTAL
            FROM LG_001_01_ORFICHE
            ORDER BY LOGICALREF DESC", con);

        using SqlDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            list.Add(new OrderHeaderModel
            {
                LOGICALREF = (int)dr["LOGICALREF"],
                FICHENO = dr["FICHENO"].ToString(),
                DATE_ = (DateTime)dr["DATE_"],
                TIME_ = Convert.ToInt32(dr["TIME_"]),
                CLIENTREF = Convert.ToInt32(dr["CLIENTREF"]),
                NETTOTAL = Convert.ToDecimal(dr["NETTOTAL"])
            });
        }

        return list;
    }

    public void CreateOrder(OrderCreateViewModel model)
    {
        using SqlConnection con = new SqlConnection(_cs);
        con.Open();

        using SqlTransaction tran = con.BeginTransaction();

        try
        {
            // HEADER
            SqlCommand cmdHeader = new SqlCommand(@"
                INSERT INTO LG_001_01_ORFICHE
                (
                 TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
                 GROSSTOTAL, TOTALVAT, NETTOTAL,
                 REPORTNET, ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED, REPORTRATE,
                 BRANCH, DEPARTMENT,
                 STATUS, RECSTATUS, CANCELLED
                )
                OUTPUT INSERTED.LOGICALREF
                VALUES
                (
                 @TRCODE, @FICHENO, @DATE_, @TIME_, @CLIENTREF,
                 @GROSSTOTAL, @TOTALVAT, @NETTOTAL,
                 @REPORTNET, @ADDDISCOUNTS, @TOTALDISCOUNTS,
                 @TOTALDISCOUNTED, 1,
                 0, 0,
                 1, 1, 0
                )", con, tran);

            var h = model.Header;

            cmdHeader.Parameters.AddWithValue("@TRCODE", h.TRCODE);
            cmdHeader.Parameters.AddWithValue("@FICHENO", h.FICHENO ?? (object)DBNull.Value);
            cmdHeader.Parameters.AddWithValue("@DATE_", h.DATE_);
            cmdHeader.Parameters.AddWithValue("@TIME_", h.TIME_);
            cmdHeader.Parameters.AddWithValue("@CLIENTREF", h.CLIENTREF);
            cmdHeader.Parameters.AddWithValue("@GROSSTOTAL", h.GROSSTOTAL);
            cmdHeader.Parameters.AddWithValue("@TOTALVAT", h.TOTALVAT);
            cmdHeader.Parameters.AddWithValue("@NETTOTAL", h.NETTOTAL);
            cmdHeader.Parameters.AddWithValue("@REPORTNET", h.REPORTNET);
            cmdHeader.Parameters.AddWithValue("@ADDDISCOUNTS", h.ADDDISCOUNTS);
            cmdHeader.Parameters.AddWithValue("@TOTALDISCOUNTS", h.TOTALDISCOUNTS);
            cmdHeader.Parameters.AddWithValue("@TOTALDISCOUNTED", h.TOTALDISCOUNTED);

            int ordFicheRef = (int)cmdHeader.ExecuteScalar();

            // LINES
            int lineNo = 1;
            foreach (var line in model.Lines)
            {
                decimal total = line.AMOUNT * line.PRICE;
                decimal vatAmount = total * line.VAT / 100;

                SqlCommand cmdLine = new SqlCommand(@"
                    INSERT INTO LG_001_01_ORFLINE
                    (
                     ORDFICHEREF, STOCKREF, CLIENTREF,
                     LINETYPE, LINENO_, DETLINE,
                     TRCODE, DATE_, TIME_,
                     AMOUNT, PRICE, TOTAL,
                     VAT, VATAMNT, VATMATRAH, LINENET,
                     UOMREF, USREF,
                     RECSTATUS, CANCELLED, 
                     UINFO1, UINFO2, GROSSUINFO1, GROSSUINFO2, 
                    SHIPPEDAMOUNT, CLOSED, DORESERVE
                    )
                    VALUES
                    (
                     @ORDFICHEREF, @STOCKREF, @CLIENTREF,
                     0, @LINENO_, 0,
                     @TRCODE, @DATE_, @TIME_,
                     @AMOUNT, @PRICE, @TOTAL,
                     @VAT, @VATAMNT, @VATMATRAH, @LINENET,
                     @UOMREF, @USREF,
                     1, 0,
                     @UINFO1, @UINFO2, @GROSSUINFO1, @GROSSUINFO2, @SHIPPEDAMOUNT, @CLOSED, @DORESERVE
                    )", con, tran);

                cmdLine.Parameters.AddWithValue("@ORDFICHEREF", ordFicheRef);
                cmdLine.Parameters.AddWithValue("@STOCKREF", line.STOCKREF);
                cmdLine.Parameters.AddWithValue("@CLIENTREF", h.CLIENTREF);
                cmdLine.Parameters.AddWithValue("@LINENO_", lineNo++);
                cmdLine.Parameters.AddWithValue("@TRCODE", h.TRCODE);
                cmdLine.Parameters.AddWithValue("@DATE_", h.DATE_);
                cmdLine.Parameters.AddWithValue("@TIME_", h.TIME_);
                cmdLine.Parameters.AddWithValue("@AMOUNT", line.AMOUNT);
                cmdLine.Parameters.AddWithValue("@PRICE", line.PRICE);
                cmdLine.Parameters.AddWithValue("@TOTAL", total);
                cmdLine.Parameters.AddWithValue("@VAT", line.VAT);
                cmdLine.Parameters.AddWithValue("@VATAMNT", vatAmount);
                cmdLine.Parameters.AddWithValue("@VATMATRAH", total);
                cmdLine.Parameters.AddWithValue("@LINENET", total);
                cmdLine.Parameters.AddWithValue("@UOMREF", line.UOMREF);
                cmdLine.Parameters.AddWithValue("@USREF", line.USREF);
              
                cmdLine.Parameters.AddWithValue("@UINFO1", 1);
                cmdLine.Parameters.AddWithValue("@UINFO2", 1);

                cmdLine.Parameters.AddWithValue("@GROSSUINFO1", line.AMOUNT);
                cmdLine.Parameters.AddWithValue("@GROSSUINFO2", 1);

                cmdLine.Parameters.AddWithValue("@SHIPPEDAMOUNT", line.AMOUNT); // ✅ önemli
                cmdLine.Parameters.AddWithValue("@CLOSED", 0);
                cmdLine.Parameters.AddWithValue("@DORESERVE", 0);

                cmdLine.ExecuteNonQuery();


            }
            // 🔹 İNDİRİM SATIRI EKLE
            decimal discountAmount = 0;
            if (model.Header.TOTALDISCOUNTS > 0)
            {
                discountAmount = model.Header.TOTALDISCOUNTS;

                SqlCommand cmdDiscount = new SqlCommand(@"
                    INSERT INTO LG_001_01_ORFLINE (
                     ORDFICHEREF, STOCKREF, CLIENTREF,  LINETYPE, LINENO_, DETLINE,
                     TRCODE, DATE_, TIME_, AMOUNT, PRICE, TOTAL,
                     VAT, VATAMNT, VATMATRAH, LINENET,  UOMREF, USREF,
                     UINFO1, UINFO2, GROSSUINFO1, GROSSUINFO2,
                     SHIPPEDAMOUNT, CLOSED, DORESERVE,
                     RECSTATUS, CANCELLED )
                    VALUES  (
                     @ORDFICHEREF, 0, @CLIENTREF, 2, @LINENO_, 0,
                     @TRCODE, @DATE_, @TIME_, 0, 0, @TOTAL,
                     0, 0, 0, 0, 0, 0,
                     0, 0, 0, 0,  0,
                     0, 0, 1, 0
                    )
                ", con, tran);

                cmdDiscount.Parameters.AddWithValue("@ORDFICHEREF", ordFicheRef);
                cmdDiscount.Parameters.AddWithValue("@CLIENTREF", model.Header.CLIENTREF);
                cmdDiscount.Parameters.AddWithValue("@LINENO_", lineNo++);
                cmdDiscount.Parameters.AddWithValue("@TRCODE", model.Header.TRCODE);
                cmdDiscount.Parameters.AddWithValue("@DATE_", model.Header.DATE_);
                cmdDiscount.Parameters.AddWithValue("@TIME_", model.Header.TIME_);
                cmdDiscount.Parameters.AddWithValue("@TOTAL", discountAmount);

                cmdDiscount.ExecuteNonQuery();
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
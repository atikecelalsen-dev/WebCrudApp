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

    public List<OrderItemViewModel> GetItems()
    {
        var list = new List<OrderItemViewModel>();
        using var con = new SqlConnection(_cs);
        con.Open();

        using var cmd = new SqlCommand(@"
        SELECT
            i.LOGICALREF,
            i.CODE AS MalzemeKodu,
            i.NAME AS MalzemeAdi,
            g.ONHAND AS FiiliStok,
            (g.ONHAND - g.RESERVED) AS GercekStok,
            (g.ONHAND - g.RESERVED - g.ACTSORDER) AS SevkedilebilirStok
        FROM LG_001_ITEMS i
        JOIN LV_001_01_GNTOTST g
            ON i.LOGICALREF = g.STOCKREF
        WHERE g.INVENNO = 0
        ORDER BY i.LOGICALREF
    ", con);

        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            list.Add(new OrderItemViewModel
            {
                Value = dr["LOGICALREF"].ToString(),
                Code = dr["MalzemeKodu"].ToString(),
                Text = dr["MalzemeAdi"].ToString(),
                OnHand = Convert.ToDecimal(dr["FiiliStok"]),
                RealStock = Convert.ToDecimal(dr["GercekStok"]),
                ShippableStock = Convert.ToDecimal(dr["SevkedilebilirStok"])
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

        //SqlCommand cmd = new SqlCommand(@"
        //    SELECT TOP 1000 LOGICALREF, FICHENO, DATE_, TIME_, CLIENTREF, NETTOTAL
        //    FROM LG_001_01_ORFICHE
        //    ORDER BY LOGICALREF DESC", con);
        SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 1000
                O.LOGICALREF,
                O.FICHENO,
                O.DATE_,
                O.TIME_,
                O.CLIENTREF,
                C.DEFINITION_ AS CLIENTNAME,
                O.NETTOTAL
            FROM LG_001_01_ORFICHE O
            LEFT JOIN LG_001_CLCARD C
                ON C.LOGICALREF = O.CLIENTREF
            ORDER BY O.LOGICALREF DESC
        ", con);


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
                CLIENTNAME= dr["CLIENTNAME"].ToString(),
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

    public OrderCreateViewModel GetOrderForEdit(int ordFicheRef)
    {
        var model = new OrderCreateViewModel();

        using var con = new SqlConnection(_cs);
        con.Open();

        // ================= HEADER =================
        using (var cmd = new SqlCommand(@"
        SELECT 
            LOGICALREF, TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
            GROSSTOTAL, TOTALVAT, NETTOTAL,
            ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED, REPORTNET
            FROM LG_001_01_ORFICHE WHERE LOGICALREF = @ID
        ", con))
            {
            cmd.Parameters.AddWithValue("@ID", ordFicheRef);

            using var dr = cmd.ExecuteReader();
            if (!dr.Read()) return null;

            model.Header = new OrderHeaderModel
            {
                LOGICALREF = (int)dr["LOGICALREF"],
                TRCODE = Convert.ToInt16(dr["TRCODE"]),
                FICHENO = dr["FICHENO"].ToString(),
                DATE_ = (DateTime)dr["DATE_"],
                TIME_ = Convert.ToInt32(dr["TIME_"]),
                CLIENTREF = Convert.ToInt32(dr["CLIENTREF"]),
                GROSSTOTAL = Convert.ToDecimal(dr["GROSSTOTAL"]),
                TOTALVAT = Convert.ToDecimal(dr["TOTALVAT"]),
                NETTOTAL = Convert.ToDecimal(dr["NETTOTAL"]),
                ADDDISCOUNTS = Convert.ToDecimal(dr["ADDDISCOUNTS"]),
                TOTALDISCOUNTS = Convert.ToDecimal(dr["TOTALDISCOUNTS"]),
                TOTALDISCOUNTED = Convert.ToDecimal(dr["TOTALDISCOUNTED"]),
                REPORTNET = Convert.ToDecimal(dr["REPORTNET"])
            };
        }

        // ================= LINES =================
        model.Lines = new List<OrderLineModel>();

        using (var cmd = new SqlCommand(@"
        SELECT 
                STOCKREF, AMOUNT, PRICE, VAT,
                UOMREF, USREF,
                LINENET, VATAMNT
            FROM LG_001_01_ORFLINE
            WHERE ORDFICHEREF = @ID
              AND LINETYPE = 0
            ORDER BY LINENO_
        ", con))
            {
            cmd.Parameters.AddWithValue("@ID", ordFicheRef);

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                model.Lines.Add(new OrderLineModel
                {
                    STOCKREF = Convert.ToInt32(dr["STOCKREF"]),
                    AMOUNT = Convert.ToDecimal(dr["AMOUNT"]),
                    PRICE = Convert.ToDecimal(dr["PRICE"]),
                    VAT = Convert.ToInt32(dr["VAT"]),
                    VATAMNT = Convert.ToDecimal(dr["VATAMNT"]),
                    LINENET = Convert.ToDecimal(dr["LINENET"]),
                    UOMREF = Convert.ToInt32(dr["UOMREF"]),
                    USREF = Convert.ToInt32(dr["USREF"])
                });
            }
        }

        return model;
    }




}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebCrudApp.Data;
using WebCrudApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebCrudApp.Repository
{
    public class ItemDetailRepository
    {
        public ItemViewModel GetItemDetails(int logicalRef)
        {
            string sql = @"
                SELECT i.LOGICALREF, i.CODE, i.NAME, i.UNITSETREF,
                u.NAME AS UNITNAME FROM LG_001_ITEMS i
                LEFT JOIN LG_001_UNITSETF u ON i.UNITSETREF = u.LOGICALREF
                WHERE i.LOGICALREF = @id";

            DataTable dt = SqlHelper.Select(sql,
                new SqlParameter("@id", logicalRef));

            if (dt.Rows.Count == 0)
                return null;

            DataRow dr = dt.Rows[0];

            return new ItemViewModel
            {
                LOGICALREF = Convert.ToInt32(dr["LOGICALREF"]),
                CODE = dr["CODE"].ToString(),
                NAME = dr["NAME"].ToString(),
                UNITSETREF = Convert.ToInt32(dr["UNITSETREF"]),
                UNITNAME = dr["UNITNAME"]?.ToString() ?? ""
            };
        }

        public List<ItemUnitDetailModel> GetItemUnitDetails(int logicalRef)
        {
            string sql = @"SELECT 
                i.NAME AS ITEMNAME, u.LOGICALREF AS ITMUNITAREF ,u.ITEMREF, u.UNITLINEREF,
                ul.NAME AS UNITNAME, b.BARCODE, ps.PRICE AS SALEPRICE, pp.PRICE AS PURCHASEPRICE
            FROM LG_001_ITMUNITA u
            LEFT JOIN LG_001_ITEMS i ON i.LOGICALREF = u.ITEMREF
            LEFT JOIN LG_001_UNITSETL ul ON u.UNITLINEREF = ul.LOGICALREF
            LEFT JOIN LG_001_UNITBARCODE b ON b.ITEMREF = u.ITEMREF AND b.ITMUNITAREF = u.LOGICALREF
            LEFT JOIN LG_001_PRCLIST ps ON ps.CARDREF = u.ITEMREF AND ps.UOMREF = u.UNITLINEREF AND ps.PTYPE = 2
            LEFT JOIN LG_001_PRCLIST pp ON pp.CARDREF = u.ITEMREF AND pp.UOMREF = u.UNITLINEREF AND pp.PTYPE = 1
            WHERE u.ITEMREF = @id";

            DataTable dt = SqlHelper.Select(sql, new SqlParameter("@id", logicalRef));

            List<ItemUnitDetailModel> list = new List<ItemUnitDetailModel>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new ItemUnitDetailModel
                {
                    LOGICALREF = Convert.ToInt32(dr["ITMUNITAREF"]),
                    ITEMNAME = dr["ITEMNAME"]?.ToString() ?? "",
                    ITEMREF = Convert.ToInt32(dr["ITEMREF"]),
                    ITMUNITAREF = Convert.ToInt32(dr["ITMUNITAREF"]),
                    UNITLINEREF = Convert.ToInt32(dr["UNITLINEREF"]),
                    UNITNAME = dr["UNITNAME"]?.ToString() ?? "",
                    BARCODE = dr["BARCODE"]?.ToString() ?? "",
                    SALEPRICE = dr["SALEPRICE"] != DBNull.Value ? Convert.ToDecimal(dr["SALEPRICE"]) : 0,
                    PURCHASEPRICE = dr["PURCHASEPRICE"] != DBNull.Value ? Convert.ToDecimal(dr["PURCHASEPRICE"]) : 0
                });
            }

            return list;
        }

        [HttpPost]
        public void UpdateItems(List<ItemUnitDetailModel> items)
        {
            using SqlConnection conn = new SqlConnection(SqlHelper.connStr);
            conn.Open();
            int lineNr = 1;

            foreach (var item in items)
            {
                using var tran = conn.BeginTransaction();
                try
                {
                    // ===== BARCODE =====
                    if (!string.IsNullOrEmpty(item.BARCODE))
                    {
                        string sqlBarcode = @"
                            IF EXISTS(SELECT 1 FROM LG_001_UNITBARCODE 
                                      WHERE ITEMREF=@itemref AND ITMUNITAREF=@itmunitaref)
                            BEGIN
                                UPDATE LG_001_UNITBARCODE
                                SET BARCODE=@barcode
                                WHERE ITEMREF=@itemref AND UNITLINEREF=@unitlineref
                            END
                            ELSE
                            BEGIN
                                INSERT INTO LG_001_UNITBARCODE
                                (ITEMREF, ITMUNITAREF, UNITLINEREF, BARCODE, LINENR)
                                VALUES (@itemref, @itmunitaref, @unitlineref, @barcode, @linenr)
                            END";

                        SqlHelper.Execute(sqlBarcode, conn, tran,
                            new SqlParameter("@itemref", item.ITEMREF),
                            new SqlParameter("@itmunitaref", item.ITMUNITAREF),
                            new SqlParameter("@unitlineref", item.UNITLINEREF),
                            new SqlParameter("@barcode", item.BARCODE),
                            new SqlParameter("@linenr", lineNr++));
                    }

                    // ===== PURCHASE PRICE (PTYPE=1) =====
                    if (item.PURCHASEPRICE.HasValue && item.PURCHASEPRICE.Value > 0)
                    {
                        string sqlPurchase = @"
                            DECLARE @code NVARCHAR(20);

                            -- Satış fiyatı code'unu al
                            SELECT TOP 1 @code = CODE 
                            FROM LG_001_PRCLIST WITH (NOLOCK)
                            WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE=2;

                            IF @code IS NULL
                                SET @code = 'UOM' + CAST(ISNULL((SELECT MAX(CAST(SUBSTRING(CODE,4,10) AS INT)) 
                                                                 FROM LG_001_PRCLIST WITH (NOLOCK) WHERE CODE LIKE 'UOM%'),0)+1 AS NVARCHAR(10));

                            IF EXISTS(SELECT 1 FROM LG_001_PRCLIST WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE=1)
                            BEGIN
                                UPDATE LG_001_PRCLIST
                                SET PRICE=@price
                                WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE=1
                            END
                            ELSE
                            BEGIN
                                INSERT INTO LG_001_PRCLIST
                                (CARDREF, UOMREF, PTYPE, PRICE, CURRENCY, CODE)
                                VALUES (@cardref, @uomref, 1, @price, 160, @code)
                            END";

                        SqlHelper.Execute(sqlPurchase, conn, tran,
                            new SqlParameter("@cardref", item.ITEMREF),
                            new SqlParameter("@uomref", item.UNITLINEREF),
                            new SqlParameter("@price", item.PURCHASEPRICE));
                    }

                    // ===== SALE PRICE (PTYPE=2) =====
                    if (item.SALEPRICE.HasValue && item.SALEPRICE.Value > 0)
                    {
                        string sqlSale = @"
                            DECLARE @code NVARCHAR(20);

                            -- Alış fiyatı code'unu al
                            SELECT TOP 1 @code = CODE 
                            FROM LG_001_PRCLIST WITH (NOLOCK)
                            WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE=1;

                            IF @code IS NULL
                                SET @code = 'UOM' + CAST(ISNULL((SELECT MAX(CAST(SUBSTRING(CODE,4,10) AS INT)) 
                                                                 FROM LG_001_PRCLIST WITH (NOLOCK) WHERE CODE LIKE 'UOM%'),0)+1 AS NVARCHAR(10));

                            IF EXISTS(SELECT 1 FROM LG_001_PRCLIST WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE=2)
                            BEGIN
                                UPDATE LG_001_PRCLIST
                                SET PRICE=@price
                                WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE=2
                            END
                            ELSE
                            BEGIN
                                INSERT INTO LG_001_PRCLIST
                                (CARDREF, UOMREF, PTYPE, PRICE, CURRENCY, CODE)
                                VALUES (@cardref, @uomref, 2, @price, 160, @code)
                            END";

                        SqlHelper.Execute(sqlSale, conn, tran,
                            new SqlParameter("@cardref", item.ITEMREF),
                            new SqlParameter("@uomref", item.UNITLINEREF),
                            new SqlParameter("@price", item.SALEPRICE));
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
}

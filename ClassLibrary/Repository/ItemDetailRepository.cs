
using Microsoft.Data.SqlClient;
using System.Data;
using Library.Data;
using Library.Models.Item;

namespace Library.Repository
{
    public class ItemDetailRepository
    {
        public ItemViewModel? GetItemDetails(int logicalRef)
        {
            string sql = @"
                SELECT i.LOGICALREF, i.CODE, i.NAME, i.UNITSETREF,
                       u.NAME AS UNITNAME
                FROM LG_001_ITEMS i
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
                CODE = dr["CODE"]?.ToString() ?? "",
                NAME = dr["NAME"]?.ToString() ?? "",
                UNITSETREF = Convert.ToInt32(dr["UNITSETREF"]),
                UNITNAME = dr["UNITNAME"]?.ToString() ?? ""
            };
        }

        public List<ItemUnitDetailModel> GetItemUnitDetails(int logicalRef)
        {
            string sql = @"
                SELECT 
                    i.NAME AS ITEMNAME,
                    u.LOGICALREF AS ITMUNITAREF,
                    u.ITEMREF,
                    u.UNITLINEREF,
                    ul.NAME AS UNITNAME,
                    b.BARCODE,
                    ps.PRICE AS SALEPRICE,
                    pp.PRICE AS PURCHASEPRICE
                FROM LG_001_ITMUNITA u
                LEFT JOIN LG_001_ITEMS i ON i.LOGICALREF = u.ITEMREF
                LEFT JOIN LG_001_UNITSETL ul ON u.UNITLINEREF = ul.LOGICALREF
                LEFT JOIN LG_001_UNITBARCODE b 
                       ON b.ITEMREF = u.ITEMREF AND b.ITMUNITAREF = u.LOGICALREF
                LEFT JOIN LG_001_PRCLIST ps 
                       ON ps.CARDREF = u.ITEMREF AND ps.UOMREF = u.UNITLINEREF AND ps.PTYPE = 2
                LEFT JOIN LG_001_PRCLIST pp 
                       ON pp.CARDREF = u.ITEMREF AND pp.UOMREF = u.UNITLINEREF AND pp.PTYPE = 1
                WHERE u.ITEMREF = @id";

            DataTable dt = SqlHelper.Select(sql,
                new SqlParameter("@id", logicalRef));

            List<ItemUnitDetailModel> list = new();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new ItemUnitDetailModel
                {
                    LOGICALREF = Convert.ToInt32(dr["ITMUNITAREF"]),
                    ITEMREF = Convert.ToInt32(dr["ITEMREF"]),
                    ITMUNITAREF = Convert.ToInt32(dr["ITMUNITAREF"]),
                    UNITLINEREF = Convert.ToInt32(dr["UNITLINEREF"]),
                    ITEMNAME = dr["ITEMNAME"]?.ToString() ?? "",
                    UNITNAME = dr["UNITNAME"]?.ToString() ?? "",
                    BARCODE = dr["BARCODE"]?.ToString() ?? "",
                    SALEPRICE = dr["SALEPRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["SALEPRICE"]),
                    PURCHASEPRICE = dr["PURCHASEPRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PURCHASEPRICE"])
                });
            }

            return list;
        }

        public void UpdateItems(List<ItemUnitDetailModel> items)
        {
            using SqlConnection conn = new(SqlHelper.connStr);
            conn.Open();

            using SqlTransaction tran = conn.BeginTransaction();

            try
            {
                int lineNr = 1;

                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.BARCODE))
                    {
                        SqlHelper.Execute(@"
                            IF EXISTS(SELECT 1 FROM LG_001_UNITBARCODE 
                                      WHERE ITEMREF=@itemref AND ITMUNITAREF=@itmunitaref)
                                UPDATE LG_001_UNITBARCODE
                                SET BARCODE=@barcode
                                WHERE ITEMREF=@itemref AND ITMUNITAREF=@itmunitaref
                            ELSE
                                INSERT INTO LG_001_UNITBARCODE
                                (ITEMREF, ITMUNITAREF, UNITLINEREF, BARCODE, LINENR)
                                VALUES (@itemref, @itmunitaref, @unitlineref, @barcode, @linenr)",
                            conn, tran,
                            new SqlParameter("@itemref", item.ITEMREF),
                            new SqlParameter("@itmunitaref", item.ITMUNITAREF),
                            new SqlParameter("@unitlineref", item.UNITLINEREF),
                            new SqlParameter("@barcode", item.BARCODE),
                            new SqlParameter("@linenr", lineNr++));
                    }

                    if (item.PURCHASEPRICE.HasValue && item.PURCHASEPRICE > 0)
                    {
                        SqlHelper.Execute(GetPriceSql(1), conn, tran,
                            new SqlParameter("@cardref", item.ITEMREF),
                            new SqlParameter("@uomref", item.UNITLINEREF),
                            new SqlParameter("@price", item.PURCHASEPRICE));
                    }

                    if (item.SALEPRICE.HasValue && item.SALEPRICE > 0)
                    {
                        SqlHelper.Execute(GetPriceSql(2), conn, tran,
                            new SqlParameter("@cardref", item.ITEMREF),
                            new SqlParameter("@uomref", item.UNITLINEREF),
                            new SqlParameter("@price", item.SALEPRICE));
                    }
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        private static string GetPriceSql(int ptype) => $@"
            DECLARE @code NVARCHAR(20);

            SELECT TOP 1 @code = CODE 
            FROM LG_001_PRCLIST WITH (NOLOCK)
            WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE={(ptype == 1 ? 2 : 1)};

            IF @code IS NULL
                SET @code = 'UOM' + CAST(
                    ISNULL((SELECT MAX(CAST(SUBSTRING(CODE,4,10) AS INT)) 
                            FROM LG_001_PRCLIST WHERE CODE LIKE 'UOM%'),0)+1 
                    AS NVARCHAR(10));

            IF EXISTS(SELECT 1 FROM LG_001_PRCLIST 
                      WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE={ptype})
                UPDATE LG_001_PRCLIST
                SET PRICE=@price
                WHERE CARDREF=@cardref AND UOMREF=@uomref AND PTYPE={ptype}
            ELSE
                INSERT INTO LG_001_PRCLIST
                (CARDREF, UOMREF, PTYPE, PRICE, CURRENCY, CODE, ACTIVE)
                VALUES (@cardref, @uomref, {ptype}, @price, 160, @code, 0)";
    }
}

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





    }
}

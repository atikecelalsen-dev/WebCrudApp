using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using WebCrudApp.Data;
using WebCrudApp.Models;
using WebCrudApp.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebCrudApp.Controllers
{
    public class ItemDetailsController : Controller
    {
        ItemDetailRepository repo = new ItemDetailRepository();

        public IActionResult Index(int logicalRef)
        {
            ItemViewModel model = repo.GetItemDetails(logicalRef);
            if (model == null)
                return NotFound();

            model.UnitDetails = repo.GetItemUnitDetails(logicalRef);  // bu satır eklendi

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateItems([FromBody] List<ItemUnitDetailModel> items)
        {
            repo.UpdateItems(items);
            return Ok();
        }




        //[HttpPost]
        //public IActionResult UpdateItems2([FromBody] List<ItemUnitDetailModel> items)
        //{
        //    SqlConnection conn = new SqlConnection(SqlHelper.connStr);
        //    conn.Open();
        //    int lineNr = 1;


        //        foreach (var item in items){
        //        using (var tran = conn.BeginTransaction())
        //        {
        //            string sqlFindPurchaseCode = @"SELECT TOP 1 CODE FROM LG_001_PRCLIST
        //                            WITH (NOLOCK) WHERE CARDREF = @cardref 
        //                            AND UOMREF = @uomref AND PTYPE=@ptype";
        //            object obj1 = SqlHelper.Scalar(sqlFindPurchaseCode, conn, tran,
        //            new SqlParameter("@cardref", item.ITEMREF),
        //            new SqlParameter("@ptype", 2),
        //            new SqlParameter("@uomref", item.UNITLINEREF));

        //            string oldCode1 = (obj1 == null || obj1 == DBNull.Value) ?
        //                null : obj1.ToString();

        //            string code;


        //            string sqlFindSaleCode = @"SELECT TOP 1 CODE FROM LG_001_PRCLIST 
        //                            WITH (NOLOCK) WHERE CARDREF = @cardref
        //                            AND UOMREF = @uomref AND PTYPE=@ptype";
        //            object obj2 = SqlHelper.Scalar(sqlFindSaleCode, conn, tran,
        //            new SqlParameter("@cardref", item.ITEMREF),
        //            new SqlParameter("@ptype", 1),
        //            new SqlParameter("@uomref", item.UNITLINEREF));

        //            string oldCode2 = (obj2 == null || obj2 == DBNull.Value) ?
        //                null : obj2.ToString();


        //            string sqlMaxUOM = @"SELECT MAX(CAST(SUBSTRING(CODE, 4, 10) AS INT)) 
        //            FROM LG_001_PRCLIST WITH (NOLOCK) WHERE CODE LIKE 'UOM%'";
        //            object maxObj = SqlHelper.Scalar(sqlMaxUOM, conn, tran);
        //            int nextNo = (maxObj == null || maxObj == DBNull.Value)
        //                ? 1 : Convert.ToInt32(maxObj) + 1;
        //            string newCode = "UOM" + nextNo;

        //            try
        //            {

        //                // ===== BARCODE =====
        //                if (!string.IsNullOrEmpty(item.BARCODE))
        //                {
        //                    string checkBarcode = @"SELECT TOP 1 1 FROM LG_001_UNITBARCODE 
        //                    WITH (NOLOCK) WHERE ITEMREF = @itemref AND ITMUNITAREF = @itmunitaref";
        //                    int countBarcode = Convert.ToInt32(
        //                         SqlHelper.Scalar(checkBarcode, conn, tran,
        //                         new SqlParameter("@itemref", item.ITEMREF),
        //                        new SqlParameter("@itmunitaref", item.ITMUNITAREF)));

        //                    if (countBarcode > 0)
        //                    {

        //                        // Barkod güncelle
        //                        string sqlBarcode = @"UPDATE LG_001_UNITBARCODE 
        //                      SET BARCODE = @barcode 
        //                      WHERE ITEMREF = @itemref AND UNITLINEREF = @unitlineref";
        //                        SqlHelper.Execute(sqlBarcode,
        //                            new SqlParameter("@itemref", item.ITEMREF),
        //                            new SqlParameter("@itmunitaref", item.ITMUNITAREF),
        //                            new SqlParameter("@unitlineref", item.UNITLINEREF),
        //                            new SqlParameter("@barcode", item.BARCODE));
        //                    }
        //                    else
        //                    {
        //                        // INSERT

        //                        string sqlInsert = @"INSERT INTO LG_001_UNITBARCODE
        //                        (ITEMREF, ITMUNITAREF, UNITLINEREF, BARCODE, LINENR)
        //                        VALUES (@itemref, @itmunitaref, @unitlineref, @barcode, @linenr)";

        //                        SqlHelper.Execute(sqlInsert, conn, tran,
        //                             new SqlParameter("@itemref", item.ITEMREF),
        //                             new SqlParameter("@itmunitaref", item.ITMUNITAREF),
        //                             new SqlParameter("@unitlineref", item.UNITLINEREF),
        //                             new SqlParameter("@barcode", item.BARCODE),
        //                            new SqlParameter("@linenr", lineNr++));

        //                    }
        //                }

        //                // --- SALE PRICE (PTYPE=2) ---
        //                if (item.SALEPRICE.HasValue && item.SALEPRICE.Value > 0)
        //                {
        //                    string checkSale = @"SELECT TOP 1 1 FROM LG_001_PRCLIST WITH (NOLOCK) 
        //                        WHERE CARDREF = @cardref AND UOMREF = @uomref AND PTYPE = 2";

        //                    int countSale = Convert.ToInt32(SqlHelper.Scalar(checkSale, conn, tran,
        //                        new SqlParameter("@cardref", item.ITEMREF),
        //                        new SqlParameter("@uomref", item.UNITLINEREF)));

        //                    if (countSale > 0)
        //                    {
        //                        // Satış fiyatı güncelle
        //                        string sqlSale = @"UPDATE LG_001_PRCLIST 
        //                SET PRICE = @price 
        //                WHERE CARDREF = @itemref AND UOMREF = @unitlineref AND PTYPE = 2";
        //                        SqlHelper.Execute(sqlSale,
        //                            new SqlParameter("@price", item.SALEPRICE),
        //                            new SqlParameter("@itemref", item.ITEMREF),
        //                            new SqlParameter("@unitlineref", item.UNITLINEREF));
        //                    }
        //                    else
        //                    {
        //                        string sqlInsertSale = @"INSERT INTO LG_001_PRCLIST 
        //                                (CARDREF, UOMREF, PTYPE, PRICE, CURRENCY, CODE) 
        //                                VALUES (@cardref, @uomref, 2, @price, 160, @code)";

        //                        if (!string.IsNullOrEmpty(oldCode2)) { code = oldCode2; }
        //                        else { code = newCode; }

        //                        SqlHelper.Execute(sqlInsertSale, conn, tran,
        //                            new SqlParameter("@cardref", item.ITEMREF),
        //                            new SqlParameter("@uomref", item.UNITLINEREF),
        //                            new SqlParameter("@code", code),
        //                            new SqlParameter("@price", item.SALEPRICE));


        //                    }
        //                }

        //                // --- PURCHASE PRICE (PTYPE=1) ---
        //                if (item.PURCHASEPRICE.HasValue && item.PURCHASEPRICE.Value > 0)
        //                {
        //                    string checkPurchase = @"SELECT TOP 1 1 FROM LG_001_PRCLIST 
        //                        WITH (NOLOCK) WHERE CARDREF = @cardref 
        //                        AND UOMREF = @uomref AND PTYPE = 1";
        //                    int countPurchase = Convert.ToInt32(SqlHelper.Scalar(checkPurchase, conn, tran,
        //                        new SqlParameter("@cardref", item.ITEMREF),
        //                        new SqlParameter("@uomref", item.UNITLINEREF)));

        //                    if (countPurchase > 0)
        //                    {
        //                        // Alış fiyatı güncelle
        //                        string sqlPurchase = @"UPDATE LG_001_PRCLIST 
        //                       SET PRICE = @price 
        //                       WHERE CARDREF = @itemref AND UOMREF = @unitlineref AND PTYPE = 1";
        //                        SqlHelper.Execute(sqlPurchase,
        //                            new SqlParameter("@price", item.PURCHASEPRICE),
        //                            new SqlParameter("@itemref", item.ITEMREF),
        //                            new SqlParameter("@unitlineref", item.UNITLINEREF));
        //                    }
        //                    else
        //                    {
        //                        string sqlInsertPurchase = @"INSERT INTO LG_001_PRCLIST 
        //                                        (CARDREF, UOMREF, PTYPE, PRICE, CURRENCY, CODE) 
        //                                        VALUES (@cardref, @uomref, 1, @price, 160, @code)";


        //                        if (!string.IsNullOrEmpty(oldCode1)) { code = oldCode1; }
        //                        else { code = newCode; }

        //                        SqlHelper.Execute(sqlInsertPurchase, conn, tran,
        //                            new SqlParameter("@cardref", item.ITEMREF),
        //                            new SqlParameter("@uomref", item.UNITLINEREF),
        //                            new SqlParameter("@code", code),
        //                            new SqlParameter("@price", item.PURCHASEPRICE));
        //                    }
        //                }




        //                tran.Commit();
        //            }


        //            catch
        //            {
        //                tran.Rollback();
        //                throw;

        //            }
        //        }
        //    }

        //    return Ok();
        //}

    }
}

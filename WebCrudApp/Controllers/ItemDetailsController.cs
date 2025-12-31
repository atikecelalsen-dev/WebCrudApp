using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebCrudApp.Data;
using WebCrudApp.Models;
using WebCrudApp.Repository;

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
            SqlConnection conn = new SqlConnection(SqlHelper.connStr);
            conn.Open();
            var tran = conn.BeginTransaction();

            try
            {
                foreach (var item in items)
                {
                    Console.WriteLine("Itmunitaref: " + item.ITMUNITAREF);
                    Console.WriteLine("Itemref: " + item.ITEMREF);
                    // ===== BARCODE =====
                    string checkBarcode = @"SELECT COUNT(*) FROM LG_001_UNITBARCODE 
                                WHERE ITEMREF = @itemref AND ITMUNITAREF = @itmunitaref";
                    int countBarcode = Convert.ToInt32(
                         SqlHelper.Scalar(checkBarcode, conn, tran,
                         new SqlParameter("@itemref", item.ITEMREF),
                        new SqlParameter("@itmunitaref", item.ITMUNITAREF)));

                    if (countBarcode > 0)
                    {

                        // Barkod güncelle
                        string sqlBarcode = @"UPDATE LG_001_UNITBARCODE 
                              SET BARCODE = @barcode 
                              WHERE ITEMREF = @itemref AND UNITLINEREF = @unitlineref";
                        SqlHelper.Execute(sqlBarcode, 
                            new SqlParameter("@itemref", item.ITEMREF),
                            new SqlParameter("@itmunitaref", item.ITMUNITAREF),
                            new SqlParameter("@unitlineref", item.UNITLINEREF),
                            new SqlParameter("@barcode", item.BARCODE));
                    }
                    else
                    {
                        // INSERT
                        string sqlInsert = @"INSERT INTO LG_001_UNITBARCODE
                                (ITEMREF, ITMUNITAREF, UNITLINEREF, BARCODE)
                                VALUES (@itemref, @itmunitaref, @unitlineref, @barcode)";

                        SqlHelper.Execute(sqlInsert, conn, tran,
                             new SqlParameter("@itemref", item.ITEMREF),
                             new SqlParameter("@itmunitaref", item.ITMUNITAREF),
                             new SqlParameter("@unitlineref", item.UNITLINEREF),
                             new SqlParameter("@barcode", item.BARCODE));

                    }

                    // --- SALE PRICE (PTYPE=2) ---
                    string checkSale = @"SELECT COUNT(*) FROM LG_001_PRCLIST 
                                WHERE CARDREF = @cardref AND UOMREF = @uomref AND PTYPE = 2";

                    int countSale = Convert.ToInt32(SqlHelper.Scalar(checkSale, conn, tran,
                        new SqlParameter("@cardref", item.ITEMREF),
                        new SqlParameter("@uomref", item.UNITLINEREF)));

                    if (countSale > 0)
                    {
                        // Satış fiyatı güncelle
                        string sqlSale = @"UPDATE LG_001_PRCLIST 
                        SET PRICE = @price 
                        WHERE CARDREF = @itemref AND UOMREF = @unitlineref AND PTYPE = 2";
                        SqlHelper.Execute(sqlSale,
                            new SqlParameter("@price", item.SALEPRICE),
                            new SqlParameter("@itemref", item.ITEMREF),
                            new SqlParameter("@unitlineref", item.UNITLINEREF));
                    }
                    else
                    {
                        string sqlInsertSale = @"INSERT INTO LG_001_PRCLIST 
                                        (CARDREF, UOMREF, PTYPE, PRICE) 
                                        VALUES (@cardref, @uomref, 2, @price)";
                        SqlHelper.Execute(sqlInsertSale, conn, tran,
                            new SqlParameter("@cardref", item.ITEMREF),
                            new SqlParameter("@uomref", item.UNITLINEREF),
                            new SqlParameter("@price", item.SALEPRICE));

                    }

                    // --- PURCHASE PRICE (PTYPE=1) ---
                    string checkPurchase = @"SELECT COUNT(*) FROM LG_001_PRCLIST 
                                WHERE CARDREF = @cardref AND UOMREF = @uomref AND PTYPE = 1";
                    int countPurchase = Convert.ToInt32(SqlHelper.Scalar(checkPurchase, conn, tran,
                        new SqlParameter("@cardref", item.ITEMREF),
                        new SqlParameter("@uomref", item.UNITLINEREF)));

                    if (countPurchase > 0)
                    {
                        // Alış fiyatı güncelle
                        string sqlPurchase = @"UPDATE LG_001_PRCLIST 
                               SET PRICE = @price 
                               WHERE CARDREF = @itemref AND UOMREF = @unitlineref AND PTYPE = 1";
                        SqlHelper.Execute(sqlPurchase,
                            new SqlParameter("@price", item.PURCHASEPRICE),
                            new SqlParameter("@itemref", item.ITEMREF),
                            new SqlParameter("@unitlineref", item.UNITLINEREF));
                    }
                    else
                    {
                        string sqlInsertPurchase = @"INSERT INTO LG_001_PRCLIST 
                                                (CARDREF, UOMREF, PTYPE, PRICE) 
                                                VALUES (@cardref, @uomref, 1, @price)";
                        SqlHelper.Execute(sqlInsertPurchase, conn, tran,
                            new SqlParameter("@cardref", item.ITEMREF),
                            new SqlParameter("@uomref", item.UNITLINEREF),
                            new SqlParameter("@price", item.PURCHASEPRICE));
                    }
                }

                tran.Commit();
            }
            catch {
                tran.Rollback();
                throw;

            }

            return Ok();
        }

    }
}

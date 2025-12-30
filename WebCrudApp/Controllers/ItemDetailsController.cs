using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using WebCrudApp.Data;
using WebCrudApp.Models;
using WebCrudApp.Repository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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
            foreach (var item in items)
            {


                // Barkod güncelle
                string sqlBarcode = @"UPDATE LG_001_UNITBARCODE 
                              SET BARCODE = @barcode 
                              WHERE ITEMREF = @itemref AND UNITLINEREF = @unitlineref";
                SqlHelper.Execute(sqlBarcode,
                    new SqlParameter("@barcode", item.BARCODE),
                    new SqlParameter("@itemref", item.ITEMREF),
                    new SqlParameter("@unitlineref", item.UNITLINEREF));

                // Satış fiyatı güncelle
                string sqlSale = @"UPDATE LG_001_PRCLIST 
                           SET PRICE = @price 
                           WHERE CARDREF = @itemref AND UOMREF = @unitlineref AND PTYPE = 2";
                SqlHelper.Execute(sqlSale,
                    new SqlParameter("@price", item.SALEPRICE),
                    new SqlParameter("@itemref", item.ITEMREF),
                    new SqlParameter("@unitlineref", item.UNITLINEREF));

                // Alış fiyatı güncelle
                string sqlPurchase = @"UPDATE LG_001_PRCLIST 
                               SET PRICE = @price 
                               WHERE CARDREF = @itemref AND UOMREF = @unitlineref AND PTYPE = 1";
                SqlHelper.Execute(sqlPurchase,
                    new SqlParameter("@price", item.PURCHASEPRICE),
                    new SqlParameter("@itemref", item.ITEMREF),
                    new SqlParameter("@unitlineref", item.UNITLINEREF));
            }

            return Ok();
        }




    }
}

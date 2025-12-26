using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace WebCrudApp.Models
{
    public class ItemListModel
    {
        public int LogicalRef { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public short Active { get; set; }
        public int UnitSetRef { get; set; }




        public static List<ItemListModel> GetItemList()
        {
            var list = new List<ItemListModel>();

            using (SqlConnection conn = new SqlConnection(ItemViewModel.ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT LOGICALREF, CODE, NAME, ACTIVE, UNITSETREF
                    FROM LG_001_ITEMS
                    ORDER BY LOGICALREF DESC
                ", conn);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ItemListModel
                    {
                        LogicalRef = Convert.ToInt32(dr["LOGICALREF"]),
                        Code = dr["CODE"].ToString(),
                        Name = dr["NAME"].ToString(),
                        Active = Convert.ToInt16(dr["ACTIVE"]),
                        UnitSetRef = Convert.ToInt32(dr["UNITSETREF"])
                    });
                }
            }

            return list;
        }

        //public static List<ItemUnitModel> GetItemUnitList()
        //{
        //    var units = new List<ItemUnitModel>();

        //    using (SqlConnection conn = new SqlConnection(ItemViewModel.ConnectionString))
        //    {
        //        conn.Open();
        //        string query = "SELECT LOGICALREF, CODE, NAME FROM LG_001_UNITSETF";
        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                units.Add(new ItemUnitModel
        //                {
        //                    LOGICALREF = reader.GetInt32(0),
        //                    CODE = reader.GetString(1),
        //                    NAME = reader.GetString(2)
        //                });
        //            }
        //        }
        //    }

        //   return View(units);
        //}


        public static string ConnectionString =
            "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static List<ItemListModel> Search(string code, string name)
        {
            var list = new List<ItemListModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
            SELECT LOGICALREF, CODE, NAME, ACTIVE, UNITSETREF
            FROM LG_001_ITEMS
            WHERE
                (@CODE = '' OR CODE LIKE '%' + @CODE + '%')
            AND (@NAME = '' OR NAME LIKE '%' + @NAME + '%')
        ", conn);

                cmd.Parameters.AddWithValue("@CODE", code ?? "");
                cmd.Parameters.AddWithValue("@NAME", name ?? "");

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new ItemListModel
                        {
                            LogicalRef = Convert.ToInt32(dr["LOGICALREF"]),
                            Code = dr["CODE"].ToString(),
                            Name = dr["NAME"].ToString(),
                            Active = Convert.ToInt16(dr["ACTIVE"]),
                            UnitSetRef = Convert.ToInt32(dr["UNITSETREF"])
                        });
                    }
                }
            }

            return list;
        }
    }
}

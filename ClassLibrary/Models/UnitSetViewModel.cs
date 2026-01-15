using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Library.Models
{
    public class UnitSetViewModel
    {
        public int LogicalRef { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }


        private static string cs =
            "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        // 🔹 Dropdown için
        public static List<SelectListItem> GetUnitSetDropdown()
        {
            var list = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT LOGICALREF, NAME
                    FROM LG_001_UNITSETF
                    ORDER BY NAME
                ", con);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new SelectListItem
                        {
                            Value = dr["LOGICALREF"].ToString(),
                            Text = dr["NAME"].ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}
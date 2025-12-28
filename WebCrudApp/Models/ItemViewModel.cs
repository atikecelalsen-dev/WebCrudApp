using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WebCrudApp.Models
{
    public class ItemViewModel
    {
        [JsonPropertyName("logicalRef")]
        public int LOGICALREF { get; set; }

        [JsonPropertyName("code")]
        public string CODE { get; set; }

        [JsonPropertyName("name")]
        public string NAME { get; set; }
        public int ACTIVE { get; set; }
        public int CARDTYPE { get; set; }

        public int CLASSTYPE { get; set; }
        public int UNITSETREF { get; set; }


        //    public List<UnitSetViewModel> UnitSets { get; set; }

        //    private static string connStr =
        //        "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        //    [HttpPost]


        //    // 🔹 Listele
        //    public static List<ItemViewModel> GetItems()
        //    {
        //        List<ItemViewModel> list = new();

        //        using SqlConnection con = new SqlConnection(connStr);
        //        SqlCommand cmd = new SqlCommand("SELECT LOGICALREF, CODE, NAME FROM LG_001_ITEMS", con);
        //        con.Open();
        //        using SqlDataReader dr = cmd.ExecuteReader();
        //        while (dr.Read())
        //        {
        //            list.Add(new ItemViewModel
        //            {
        //                LOGICALREF = dr["LOGICALREF"] != DBNull.Value ? Convert.ToInt32(dr["LOGICALREF"]) : 0,
        //                CODE = dr["CODE"].ToString(),
        //                NAME = dr["NAME"].ToString(),
        //                ACTIVE=0,
        //                CARDTYPE=1
        //});
        //        }

        //        return list;
        //    }

        //    // 🔹 Arama
        //    public static List<ItemViewModel> Search(string code, string name)
        //    {
        //        List<ItemViewModel> list = new();

        //        using SqlConnection con = new SqlConnection(connStr);
        //        SqlCommand cmd = new SqlCommand();
        //        cmd.Connection = con;

        //        string sql = "SELECT LOGICALREF, CODE, NAME FROM LG_001_ITEMS WHERE 1=1";

        //        if (!string.IsNullOrEmpty(code))
        //        {
        //            sql += " AND CODE LIKE @code";
        //            cmd.Parameters.AddWithValue("@code", "%" + code + "%");
        //        }

        //        if (!string.IsNullOrEmpty(name))
        //        {
        //            sql += " AND NAME LIKE @name";
        //            cmd.Parameters.AddWithValue("@name", "%" + name + "%");
        //        }

        //        cmd.CommandText = sql;
        //        con.Open();
        //        using SqlDataReader dr = cmd.ExecuteReader();
        //        while (dr.Read())
        //        {
        //            list.Add(new ItemViewModel
        //            {
        //                LOGICALREF = dr["LOGICALREF"] != DBNull.Value ? Convert.ToInt32(dr["LOGICALREF"]) : 0,
        //                CODE = dr["CODE"].ToString(),
        //                NAME = dr["NAME"].ToString()
        //            });
        //        }

        //        return list;
        //    }

        //    // 🔹 Create
        //    public static void Create(string code, string name, int UNITSETREF)
        //    {
        //        using SqlConnection con = new SqlConnection(connStr);
        //        con.Open();
        //        SqlTransaction tran = con.BeginTransaction();
        //        try
        //        {

        //            SqlCommand cmd = new SqlCommand(@"
        //            INSERT INTO LG_001_ITEMS (CODE, NAME, ACTIVE, CARDTYPE, CLASSTYPE, UNITSETREF)
        //            VALUES (@c, @d, @a, @cardt, @classt,  @u); 
        //            SELECT SCOPE_IDENTITY();", con, tran);

        //            cmd.Parameters.AddWithValue("@c", code);
        //            cmd.Parameters.AddWithValue("@d", name);
        //            cmd.Parameters.AddWithValue("@a", 0);
        //            cmd.Parameters.AddWithValue("@cardt", 1);
        //            cmd.Parameters.AddWithValue("@classt", 0);
        //            cmd.Parameters.AddWithValue("@u", UNITSETREF);

        //            int itemRef = Convert.ToInt32(cmd.ExecuteScalar());
        //            SqlCommand cmdclass = new SqlCommand(@"
        //            INSERT INTO LG_001_ITMCLSAS (PARENTREF, CHILDREF, UPLEVEL, SITEID, RECSTATUS, ORGLOGICREF)
        //            VALUES (@pr, @cr,  @up, @s,  @r, @o); 
        //            SELECT SCOPE_IDENTITY();", con, tran);
        //            cmdclass.Parameters.AddWithValue("@pr", 1);
        //            cmdclass.Parameters.AddWithValue("@cr", itemRef);
        //            cmdclass.Parameters.AddWithValue("@up", 0);
        //            cmdclass.Parameters.AddWithValue("@s", 0);
        //            cmdclass.Parameters.AddWithValue("@r", 0);
        //            cmdclass.Parameters.AddWithValue("@o", 0);

        //            cmdclass.ExecuteNonQuery();

        //            SqlCommand cmdUnitLines = new SqlCommand(@"
        //                SELECT LOGICALREF, CONVFACT1, CONVFACT2
        //                FROM LG_001_UNITSETL
        //                WHERE UNITSETREF = @unitSetRef
        //                ORDER BY LOGICALREF
        //            ", con, tran);

        //            cmdUnitLines.Parameters.AddWithValue("@unitSetRef", UNITSETREF);

        //            var unitLines = new List<(int UnitLineRef, double Conv1, double Conv2)>();

        //            using (SqlDataReader dr = cmdUnitLines.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    unitLines.Add((
        //                        Convert.ToInt32(dr["LOGICALREF"]),
        //                        Convert.ToDouble(dr["CONVFACT1"]),
        //                        Convert.ToDouble(dr["CONVFACT2"])
        //                    ));
        //                }
        //            }
        //            int lineNr = 1;

        //            foreach (var u in unitLines)
        //            {
        //                SqlCommand cmdItmUnit = new SqlCommand(@"
        //                    INSERT INTO LG_001_ITMUNITA
        //                    (ITEMREF, LINENR, UNITLINEREF, CONVFACT1, CONVFACT2)
        //                    VALUES
        //                    (@itemRef, @lineNr, @unitLineRef, @cf1, @cf2)
        //                ", con, tran);

        //                cmdItmUnit.Parameters.AddWithValue("@itemRef", itemRef);
        //                cmdItmUnit.Parameters.AddWithValue("@lineNr", lineNr);
        //                cmdItmUnit.Parameters.AddWithValue("@unitLineRef", u.UnitLineRef);
        //                cmdItmUnit.Parameters.AddWithValue("@cf1", u.Conv1);
        //                cmdItmUnit.Parameters.AddWithValue("@cf2", u.Conv2);

        //                cmdItmUnit.ExecuteNonQuery();
        //                lineNr++;

        //            }
        //            tran.Commit();

        //        }
        //        catch {
        //            tran.Rollback();
        //            throw;
        //        }
        //    }

        //    // 🔹 Update
        //    public static bool Update(int id, string code, string name)
        //    {
        //        using SqlConnection con = new SqlConnection(connStr);
        //        SqlCommand cmd = new SqlCommand(@"
        //            UPDATE LG_001_ITEMS
        //            SET CODE = @code, NAME = @name
        //            WHERE LOGICALREF = @id", con);

        //        cmd.Parameters.AddWithValue("@id", id);
        //        cmd.Parameters.AddWithValue("@code", code);
        //        cmd.Parameters.AddWithValue("@name", name);

        //        con.Open();
        //        int rows = cmd.ExecuteNonQuery();
        //        return rows > 0;
        //    }

        //    // 🔹 Delete
        //    public static bool Delete(int id)
        //    {
        //        using SqlConnection con = new SqlConnection(connStr);
        //        con.Open();
        //        SqlTransaction tran = con.BeginTransaction();
        //        try {

        //            SqlCommand cmd3 = new SqlCommand("DELETE FROM LG_001_ITMUNITA WHERE ITEMREF = @id", con, tran);
        //            cmd3.Parameters.AddWithValue("@id", id);
        //            cmd3.ExecuteNonQuery();

        //            SqlCommand cmd2 = new SqlCommand("DELETE FROM LG_001_ITCLSAS WHERE CHILDREF = @id", con, tran);
        //            cmd2.Parameters.AddWithValue("@id", id);
        //            cmd2.ExecuteNonQuery();


        //            int rows = new SqlCommand("DELETE FROM LG_001_ITEMS WHERE LOGICALREF = @id", con, tran) 
        //            { Parameters = { new SqlParameter("@id", id) } }.ExecuteNonQuery();
        //            //rows.Parameters.AddWithValue("@id", id);
        //            //cmd.ExecuteNonQuery();

        //           // int rows = cmd.ExecuteNonQuery();
        //            tran.Commit();
        //            return rows > 0;

        //        }
        //        catch
        //        {
        //            tran.Rollback();
        //            return false;
        //        }
        //
        //}
    } 
}



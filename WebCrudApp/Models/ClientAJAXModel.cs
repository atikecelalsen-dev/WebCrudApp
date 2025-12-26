using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WebCrudApp.Models
{
    public class ClientAJAXModel
    {
        [JsonPropertyName("logicalRef")]
        public int LOGICALREF { get; set; }

        [JsonPropertyName("code")]
        public string CODE { get; set; }

        [JsonPropertyName("definition")]
        public string DEFINITION_ { get; set; }
        public int ACTIVE { get; set; }
        public int CARDTYPE { get; set; }

        private static string connStr =
            "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        // 🔹 Listele
        public static List<ClientAJAXModel> GetClients()
        {
            List<ClientAJAXModel> list = new();

            using SqlConnection con = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand("SELECT LOGICALREF, CODE, DEFINITION_ FROM LG_001_CLCARD", con);
            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new ClientAJAXModel
                {
                    LOGICALREF = dr["LOGICALREF"] != DBNull.Value ? Convert.ToInt32(dr["LOGICALREF"]) : 0,
                    CODE = dr["CODE"].ToString(),
                    DEFINITION_ = dr["DEFINITION_"].ToString()
                });
            }

            return list;
        }

        // 🔹 Arama
        public static List<ClientAJAXModel> Search(string code, string definition)
        {
            List<ClientAJAXModel> list = new();

            using SqlConnection con = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            string sql = "SELECT LOGICALREF, CODE, DEFINITION_ FROM LG_001_CLCARD WHERE 1=1";

            if (!string.IsNullOrEmpty(code))
            {
                sql += " AND CODE LIKE @code";
                cmd.Parameters.AddWithValue("@code", "%" + code + "%");
            }

            if (!string.IsNullOrEmpty(definition))
            {
                sql += " AND DEFINITION_ LIKE @definition";
                cmd.Parameters.AddWithValue("@definition", "%" + definition + "%");
            }

            cmd.CommandText = sql;
            con.Open();
            using SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new ClientAJAXModel
                {
                    LOGICALREF = dr["LOGICALREF"] != DBNull.Value ? Convert.ToInt32(dr["LOGICALREF"]) : 0,
                    CODE = dr["CODE"].ToString(),
                    DEFINITION_ = dr["DEFINITION_"].ToString()
                });
            }

            return list;
        }

        // 🔹 Create
        public static void Create(string code, string definition)
        {
            using SqlConnection con = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO LG_001_CLCARD (CODE, DEFINITION_, ACTIVE, CARDTYPE)
                VALUES (@c, @d, @a, @ct)", con);

            cmd.Parameters.AddWithValue("@c", code);
            cmd.Parameters.AddWithValue("@d", definition);
            cmd.Parameters.AddWithValue("@a", 0);
            cmd.Parameters.AddWithValue("@ct", 3);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // 🔹 Update
        public static bool Update(int id, string code, string definition)
        {
            using SqlConnection con = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(@"
                UPDATE LG_001_CLCARD
                SET CODE = @code, DEFINITION_ = @def
                WHERE LOGICALREF = @id", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@code", code);
            cmd.Parameters.AddWithValue("@def", definition);

            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;
        }

        // 🔹 Delete
        public static bool Delete(int id)
        {
            using SqlConnection con = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand("DELETE FROM LG_001_CLCARD WHERE LOGICALREF = @id", con);
            cmd.Parameters.AddWithValue("@id", id);

            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;
        }
    }
}



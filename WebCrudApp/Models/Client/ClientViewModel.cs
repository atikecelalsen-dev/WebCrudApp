using Microsoft.Data.SqlClient;

namespace WebCrudApp.Models.Client

{
    public class ClientViewModel
    {
        public int LOGICALREF { get; set; }
        public string CODE { get; set; }
        public string DEFINITION_ { get; set; }
        public int ACTIVE{ get; set; }
        public int CARDTYPE { get; set; }

        private static string connStr =
            "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static List<ClientViewModel> GetList(string code, string definition)
        {
            List<ClientViewModel> list = new List<ClientViewModel>();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string sql = "SELECT LOGICALREF, CODE, DEFINITION_ FROM LG_001_CLCARD WHERE 1=1";
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                if (!string.IsNullOrWhiteSpace(code))
                {
                    sql += " AND CODE LIKE @CODE";
                    cmd.Parameters.AddWithValue("@CODE", "%" + code + "%");
                }

                if (!string.IsNullOrWhiteSpace(definition))
                {
                    sql += " AND DEFINITION_ LIKE @DEF";
                    cmd.Parameters.AddWithValue("@DEF", "%" + definition + "%");
                }

                cmd.CommandText = sql;
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ClientViewModel
                    {
                        LOGICALREF = Convert.ToInt32(dr["LOGICALREF"]),
                        CODE = dr["CODE"].ToString(),
                        DEFINITION_ = dr["DEFINITION_"].ToString()
                    });
                }
            }

            return list;
        }

        public static void Add(ClientViewModel client)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO LG_001_CLCARD (CODE, DEFINITION_, ACTIVE, CARDTYPE) VALUES (@CODE, @DEF, @ACT, @CTYPE)", con);

                cmd.Parameters.AddWithValue("@CODE", client.CODE);
                cmd.Parameters.AddWithValue("@DEF", client.DEFINITION_);
                cmd.Parameters.AddWithValue("@ACT", 0);
                cmd.Parameters.AddWithValue("@CTYPE", 3);


                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(ClientViewModel client)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE LG_001_CLCARD 
                      SET CODE=@CODE, DEFINITION_=@DEF 
                      WHERE LOGICALREF=@ID", con);

                cmd.Parameters.AddWithValue("@CODE", client.CODE);
                cmd.Parameters.AddWithValue("@DEF", client.DEFINITION_);
                cmd.Parameters.AddWithValue("@ID", client.LOGICALREF);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM LG_001_CLCARD WHERE LOGICALREF=@ID", con);

                cmd.Parameters.AddWithValue("@ID", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
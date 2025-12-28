using Microsoft.Data.SqlClient;
using System.Data;

namespace WebCrudApp.Data
{
    public static class SqlHelper
    {
        private static readonly string connStr =
            "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static DataTable Select(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection con = new SqlConnection(connStr);
            using SqlCommand cmd = new SqlCommand(sql, con);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int Execute(string sql, SqlConnection con, SqlTransaction tran, params SqlParameter[] parameters)
        {
            using SqlCommand cmd = new SqlCommand(sql, con, tran);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            return cmd.ExecuteNonQuery();
        }
        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection con = new SqlConnection(connStr);
            using SqlCommand cmd = new SqlCommand(sql, con);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            con.Open();
            return cmd.ExecuteNonQuery();
        }
        public static object Scalar(string sql, SqlConnection con, SqlTransaction tran, params SqlParameter[] parameters)
        {
            using SqlCommand cmd = new SqlCommand(sql, con, tran);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            return cmd.ExecuteScalar();
        }
    }
}
using Microsoft.Data.SqlClient;
using System.Data;
using WebCrudApp.Data;

namespace WebCrudApp.Models
{
    public class ItemRepository
    {
        public List<ItemViewModel> GetItems()
        {
            var list = new List<ItemViewModel>();
            DataTable dt = SqlHelper.Select(
                "SELECT LOGICALREF, CODE, NAME FROM LG_001_ITEMS");

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new ItemViewModel
                {
                    LOGICALREF = Convert.ToInt32(dr["LOGICALREF"]),
                    CODE = dr["CODE"].ToString(),
                    NAME = dr["NAME"].ToString()
                });
            }
            return list;
        }

        public List<ItemViewModel> Search(string code, string name)
        {
            string sql = "SELECT LOGICALREF, CODE, NAME FROM LG_001_ITEMS WHERE 1=1";
            List<SqlParameter> prms = new();

            if (!string.IsNullOrEmpty(code))
            {
                sql += " AND CODE LIKE @c";
                prms.Add(new SqlParameter("@c", "%" + code + "%"));
            }

            if (!string.IsNullOrEmpty(name))
            {
                sql += " AND NAME LIKE @n";
                prms.Add(new SqlParameter("@n", "%" + name + "%"));
            }

            DataTable dt = SqlHelper.Select(sql, prms.ToArray());
            var list = new List<ItemViewModel>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new ItemViewModel
                {
                    LOGICALREF = Convert.ToInt32(dr["LOGICALREF"]),
                    CODE = dr["CODE"].ToString(),
                    NAME = dr["NAME"].ToString()
                });
            }
            return list;
        }

        public void Create(string code, string name, int unitSetRef)
        {
            using SqlConnection con = new SqlConnection(
                "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                int itemRef = Convert.ToInt32(
                    SqlHelper.Scalar(@"
                        INSERT INTO LG_001_ITEMS
                        (CODE, NAME, ACTIVE, CARDTYPE, CLASSTYPE, UNITSETREF)
                        VALUES (@c,@n,0,1,0,@u);
                        SELECT SCOPE_IDENTITY();",
                        con, tran,
                        new SqlParameter("@c", code),
                        new SqlParameter("@n", name),
                        new SqlParameter("@u", unitSetRef))
                );

                SqlHelper.Execute(@"
                    INSERT INTO LG_001_ITMCLSAS
                    (PARENTREF, CHILDREF, UPLEVEL, SITEID, RECSTATUS, ORGLOGICREF)
                    VALUES (1,@i,0,0,0,0)",
                    con, tran,
                    new SqlParameter("@i", itemRef));

                DataTable unitLines = SqlHelper.Select(
                    "SELECT LOGICALREF, CONVFACT1, CONVFACT2 FROM LG_001_UNITSETL WHERE UNITSETREF=@u",
                    new SqlParameter("@u", unitSetRef));

                int lineNr = 1;
                foreach (DataRow u in unitLines.Rows)
                {
                    SqlHelper.Execute(@"
                        INSERT INTO LG_001_ITMUNITA
                        (ITEMREF, LINENR, UNITLINEREF, CONVFACT1, CONVFACT2)
                        VALUES (@i,@l,@ul,@c1,@c2)",
                        con, tran,
                        new SqlParameter("@i", itemRef),
                        new SqlParameter("@l", lineNr++),
                        new SqlParameter("@ul", u["LOGICALREF"]),
                        new SqlParameter("@c1", u["CONVFACT1"]),
                        new SqlParameter("@c2", u["CONVFACT2"])
                    );
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        //  public bool Update(int id, string code, string name)
        //{
        //      return SqlHelper.Execute(
        //          "UPDATE LG_001_ITEMS SET CODE=@c, NAME=@n WHERE LOGICALREF=@i",
        //          new SqlConnection(), null,
        //          new SqlParameter("@c", code),
        //          new SqlParameter("@n", name),
        //          new SqlParameter("@i", id)) > 0;
        //  }
        //public static bool Update(int id, string code, string name)
        //{
        //    using SqlConnection con = new SqlConnection(
        //        "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");
        //    con.Open();
        //    SqlTransaction tran = con.BeginTransaction();
        //    new SqlParameter("@i", id);

        //    try
        //    {

        //        int rows = SqlHelper.Execute("UPDATE LG_001_ITEMS SET CODE = @code, " +
        //            "NAME = @name, ACTIVE = 0, CARDTYPE = 1 " +
        //            "WHERE LOGICALREF = @i", con, tran);

        //        tran.Commit();
        //        return rows > 0;
        //    }
        //    catch
        //    {
        //       // Console.WriteLine("SQL ERROR: " + ex.Message);
        //        tran.Rollback();
        //        return false;



        //    }
        //}

        public static bool Update(int id, string code, string name)
        {
            using SqlConnection con = new SqlConnection(
                "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");
            con.Open();
            new SqlParameter("@i", id);

            SqlCommand cmd = new SqlCommand(@"
        UPDATE LG_001_ITEMS
        SET 
            CODE = @code,
            NAME = @name,
            ACTIVE = 0,
            CARDTYPE = 1
        WHERE LOGICALREF = @id", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@code", code);
            cmd.Parameters.AddWithValue("@name", name);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL ERROR: " + ex.Message);
                throw;
            }
        }

        public bool Delete(int id)
        {
            using SqlConnection con = new SqlConnection(
                "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");
            con.Open();
            SqlTransaction tran = con.BeginTransaction();

            try
            {
                SqlHelper.Execute("DELETE FROM LG_001_ITMUNITA WHERE ITEMREF=@i", con, tran,
                    new SqlParameter("@i", id));

                SqlHelper.Execute("DELETE FROM LG_001_ITMCLSAS WHERE CHILDREF=@i", con, tran,
                    new SqlParameter("@i", id));

                int rows = SqlHelper.Execute(
                    "DELETE FROM LG_001_ITEMS WHERE LOGICALREF=@i",
                    con, tran,
                    new SqlParameter("@i", id));

                tran.Commit();
                return rows > 0;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }
    }
}
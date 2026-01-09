using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using System.Data;
using WebCrudApp.Data;
using WebCrudApp.Models.Item;

namespace WebCrudApp.Models
{
    public class ItemRepository
    { 
        public List<ItemViewModel> GetItems()
        {
            string sql = @"
        SELECT i.LOGICALREF, i.CODE, i.NAME AS ITEMNAME, i.UNITSETREF, u.NAME AS UNITNAME
        FROM LG_001_ITEMS i
        LEFT JOIN LG_001_UNITSETF u ON i.UNITSETREF = u.LOGICALREF
        ORDER BY i.NAME";

            DataTable dt = SqlHelper.Select(sql);

            var list = new List<ItemViewModel>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new ItemViewModel
                {
                    LOGICALREF = Convert.ToInt32(dr["LOGICALREF"]),
                    CODE = dr["CODE"].ToString(),
                    NAME = dr["ITEMNAME"].ToString(),
                    UNITSETREF = Convert.ToInt32(dr["UNITSETREF"]),
                    UNITNAME = dr["UNITNAME"]?.ToString() ?? ""  // Burada Name dolacak
                });
            }

            return list;
        }
        public List<ItemViewModel> Search(string code, string name, int? unitSetRef)
        {
           // string sql = "SELECT LOGICALREF, CODE, NAME FROM LG_001_ITEMS WHERE 1=1";
            string sql = "SELECT i.LOGICALREF, i.CODE, i.NAME AS ITEMNAME, i.UNITSETREF," +
                " u.NAME AS UNITNAME FROM LG_001_ITEMS i LEFT JOIN LG_001_UNITSETF " +
                "u ON i.UNITSETREF = u.LOGICALREF WHERE 1 = 1";
            string sql2 = "SELECT i.LOGICALREF, i.CODE AS ITEMCODE, i.NAME AS ITEMNAME, " +
                "i.UNITSETREF, u.NAME AS UNITNAME FROM LG_001_ITEMS i " +
                "LEFT JOIN LG_001_UNITSETF u ON i.UNITSETREF = u.LOGICALREF WHERE 1=1";


            List<SqlParameter> prms = new();

            if (!string.IsNullOrEmpty(code))
            {
                sql += " AND i.CODE LIKE @code";   
                prms.Add(new SqlParameter("@code", "%" + code + "%"));
            }

            if (!string.IsNullOrEmpty(name))
            {
                sql += " AND i.NAME LIKE @name";  
                prms.Add(new SqlParameter("@name", "%" + name + "%"));
            }

            if (unitSetRef.HasValue && unitSetRef.Value != 0)
            {
                sql += " AND UNITSETREF = @u";
                prms.Add(new SqlParameter("@u", unitSetRef.Value));
            }


            DataTable dt = SqlHelper.Select(sql, prms.ToArray());
            var list = new List<ItemViewModel>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new ItemViewModel
                {
                    LOGICALREF = Convert.ToInt32(dr["LOGICALREF"]),
                    CODE = dr["CODE"].ToString(),
                    NAME = dr["ITEMNAME"].ToString(),
                    UNITSETREF = Convert.ToInt32(dr["UNITSETREF"]),
                    UNITNAME = dr["UNITNAME"]?.ToString() ?? ""
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


        public static bool Update(int itemRef, string code, string name, int unitSetRef)
        {
            using SqlConnection con = new SqlConnection(
                "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");
            con.Open();

            using SqlTransaction tran = con.BeginTransaction();
            try
            {
                // 1. ITEMS tablosunu güncelle
                int rows = SqlHelper.Execute(@"
            UPDATE LG_001_ITEMS
            SET CODE = @code,
                NAME = @name,
                ACTIVE = 0,
                CARDTYPE = 1,
                UNITSETREF = @unitSetRef
            WHERE LOGICALREF = @id",
                    con, tran,
                    new SqlParameter("@id", itemRef),
                    new SqlParameter("@code", code),
                    new SqlParameter("@name", name),
                    new SqlParameter("@unitSetRef", unitSetRef)
                );

                if (rows == 0)
                {
                    tran.Rollback();
                    return false;
                }

                // 2. Önceki ITMUNITA satırlarını sil
                SqlHelper.Execute("DELETE FROM LG_001_ITMUNITA WHERE ITEMREF=@i",
                    con, tran,
                    new SqlParameter("@i", itemRef));

                // 3. UNITSETL'den birimleri al
                DataTable unitLines = SqlHelper.Select(
                    "SELECT LOGICALREF, CONVFACT1, CONVFACT2 FROM LG_001_UNITSETL WHERE UNITSETREF=@u",
                    new SqlParameter("@u", unitSetRef));

                // 4. Yeni ITMUNITA satırlarını ekle
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
                return true;
            }
            catch (Exception ex)
            {
                tran.Rollback();
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
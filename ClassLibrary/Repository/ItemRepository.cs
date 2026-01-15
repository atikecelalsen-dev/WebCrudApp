
using System.Data;
using Microsoft.Data.SqlClient;
using Library.Data;
using Library.Models.Item;

namespace Library.Repository
{
    public class ItemRepository
    {
        // ================= GET =================
        public List<ItemViewModel> GetItems()
        {
            const string sql = @"
                SELECT i.LOGICALREF, i.CODE, i.NAME AS ITEMNAME, i.UNITSETREF, u.NAME AS UNITNAME
                FROM LG_001_ITEMS i
                LEFT JOIN LG_001_UNITSETF u ON i.UNITSETREF = u.LOGICALREF
                ORDER BY i.NAME";

            DataTable dt = SqlHelper.Select(sql);

            return dt.AsEnumerable().Select(dr => new ItemViewModel
            {
                LOGICALREF = dr.Field<int>("LOGICALREF"),
                CODE = dr.Field<string>("CODE") ?? "",
                NAME = dr.Field<string>("ITEMNAME") ?? "",
                UNITSETREF = dr.Field<int>("UNITSETREF"),
                UNITNAME = dr.Field<string>("UNITNAME") ?? ""
            }).ToList();
        }

        // ================= SEARCH =================
        public List<ItemViewModel> Search(string code, string name, int? unitSetRef)
        {
            string sql = @"
                SELECT i.LOGICALREF, i.CODE, i.NAME AS ITEMNAME, i.UNITSETREF, u.NAME AS UNITNAME
                FROM LG_001_ITEMS i
                LEFT JOIN LG_001_UNITSETF u ON i.UNITSETREF = u.LOGICALREF
                WHERE 1 = 1";

            var prms = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(code))
            {
                sql += " AND i.CODE LIKE @code";
                prms.Add(new SqlParameter("@code", $"%{code}%"));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += " AND i.NAME LIKE @name";
                prms.Add(new SqlParameter("@name", $"%{name}%"));
            }

            if (unitSetRef.HasValue && unitSetRef.Value > 0)
            {
                sql += " AND i.UNITSETREF = @u";
                prms.Add(new SqlParameter("@u", unitSetRef.Value));
            }

            DataTable dt = SqlHelper.Select(sql, prms.ToArray());

            return dt.AsEnumerable().Select(dr => new ItemViewModel
            {
                LOGICALREF = dr.Field<int>("LOGICALREF"),
                CODE = dr.Field<string>("CODE") ?? "",
                NAME = dr.Field<string>("ITEMNAME") ?? "",
                UNITSETREF = dr.Field<int>("UNITSETREF"),
                UNITNAME = dr.Field<string>("UNITNAME") ?? ""
            }).ToList();
        }

        // ================= CREATE =================
        public void Create(string code, string name, int unitSetRef)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();
            using SqlTransaction tran = con.BeginTransaction();

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

        // ================= UPDATE =================
        public static bool Update(int itemRef, string code, string name, int unitSetRef)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();
            using SqlTransaction tran = con.BeginTransaction();

            try
            {
                int rows = SqlHelper.Execute(@"
                    UPDATE LG_001_ITEMS
                    SET CODE=@code, NAME=@name, ACTIVE=0, CARDTYPE=1, UNITSETREF=@u
                    WHERE LOGICALREF=@id",
                    con, tran,
                    new SqlParameter("@id", itemRef),
                    new SqlParameter("@code", code),
                    new SqlParameter("@name", name),
                    new SqlParameter("@u", unitSetRef));

                if (rows == 0)
                {
                    tran.Rollback();
                    return false;
                }

                SqlHelper.Execute("DELETE FROM LG_001_ITMUNITA WHERE ITEMREF=@i",
                    con, tran, new SqlParameter("@i", itemRef));

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
                return true;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // ================= DELETE =================
        public bool Delete(int id)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();
            using SqlTransaction tran = con.BeginTransaction();

            try
            {
                SqlHelper.Execute("DELETE FROM LG_001_ITMUNITA WHERE ITEMREF=@i", con, tran, new SqlParameter("@i", id));
                SqlHelper.Execute("DELETE FROM LG_001_ITMCLSAS WHERE CHILDREF=@i", con, tran, new SqlParameter("@i", id));

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
                throw;
            }
        }
    }
}

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;


namespace WebCrudApp.Models
{
    public class ItemViewModel
    {
        // FORM ALANLARI
        public int LogicalRef { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string SearchText { get; set; }
        public int UnitSetRef { get; set; }
        public int UnitUsageType { get; set; } // 1: Sadece Adet, 2: Adet + Koli

        // CONNECTION STRING (Web.config YOK)
        public static string ConnectionString =
            "Data Source=Atike;Initial Catalog=GODENEME;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        // ANA INSERT METODU
        public static void InsertItem(ItemViewModel model)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    int itemLogicalRef;

                    // 1️⃣ LG_001_ITEMS
                    SqlCommand cmdItem = new SqlCommand(@"
                        INSERT INTO LG_001_ITEMS
                        (CODE, NAME, ACTIVE, CARDTYPE, CLASSTYPE, UNITSETREF)
                        VALUES
                        (@CODE, @NAME, 0, 1, 0, @UNITSETREF);

                        SELECT SCOPE_IDENTITY();
                    ", conn, tran);

                    cmdItem.Parameters.AddWithValue("@CODE", model.Code);
                    cmdItem.Parameters.AddWithValue("@NAME", model.Name);
                    cmdItem.Parameters.AddWithValue("@UNITSETREF", 5); //SIMDILIK 5

                    itemLogicalRef = Convert.ToInt32(cmdItem.ExecuteScalar());

                    // 2️⃣ LG_001_ITMCLSAS
                    SqlCommand cmdClass = new SqlCommand(@"
                        INSERT INTO LG_001_ITMCLSAS (CHILDREF)
                        VALUES (@CHILDREF)
                    ", conn, tran);

                    cmdClass.Parameters.AddWithValue("@CHILDREF", itemLogicalRef);
                    cmdClass.ExecuteNonQuery();

                    // 3️⃣ LG_001_ITMUNITA
                    if (model.UnitUsageType == 1)
                    {
                        // 🔹 SADECE ADET (UNITLINEREF = 5)
                        SqlCommand cmdUnit = new SqlCommand(@"
                            INSERT INTO LG_001_ITMUNITA
                            (ITEMREF, LINENR, UNITLINEREF, CONVFACT1, CONVFACT2)
                            VALUES
                            (@ITEMREF, 1, 5, 1, 1)
                        ", conn, tran);

                        cmdUnit.Parameters.AddWithValue("@ITEMREF", itemLogicalRef);
                        cmdUnit.ExecuteNonQuery();
                    }
                    else if (model.UnitUsageType == 2)
                    {
                        // 🔹 ADET (23)
                        SqlCommand cmdUnit1 = new SqlCommand(@"
                            INSERT INTO LG_001_ITMUNITA
                            (ITEMREF, LINENR, UNITLINEREF, CONVFACT1, CONVFACT2)
                            VALUES
                            (@ITEMREF, 1, 23, 1, 1)
                        ", conn, tran);

                        cmdUnit1.Parameters.AddWithValue("@ITEMREF", itemLogicalRef);
                        cmdUnit1.ExecuteNonQuery();

                        // 🔹 KOLİ (25) – 12 ADET
                        SqlCommand cmdUnit2 = new SqlCommand(@"
                            INSERT INTO LG_001_ITMUNITA
                            (ITEMREF, LINENR, UNITLINEREF, CONVFACT1, CONVFACT2)
                            VALUES
                            (@ITEMREF, 2, 25, 1, 12)
                        ", conn, tran);

                        cmdUnit2.Parameters.AddWithValue("@ITEMREF", itemLogicalRef);
                        cmdUnit2.ExecuteNonQuery();
                    }

                    // 4️⃣ COMMIT
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

       
        public static void UpdateItem(ItemViewModel model)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
            UPDATE LG_001_ITEMS
            SET CODE = @CODE,
                NAME = @NAME, 
                UNITSETREF = @UNITSETREF
            WHERE LOGICALREF = @ID ", conn);

              //  Unitsetref tablosunu da guncellememiz lazim

                cmd.Parameters.AddWithValue("@CODE", model.Code);
                cmd.Parameters.AddWithValue("@NAME", model.Name);
                cmd.Parameters.AddWithValue("@UNITSETREF", model.UnitSetRef);
                cmd.Parameters.AddWithValue("@ID", model.LogicalRef);

                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteItem(int logicalRef)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    new SqlCommand("DELETE FROM LG_001_ITMUNITA WHERE ITEMREF=@ID", conn, tran)
                    { Parameters = { new SqlParameter("@ID", logicalRef) } }.ExecuteNonQuery();

                    new SqlCommand("DELETE FROM LG_001_ITMCLSAS WHERE CHILDREF=@ID", conn, tran)
                    { Parameters = { new SqlParameter("@ID", logicalRef) } }.ExecuteNonQuery();

                    new SqlCommand("DELETE FROM LG_001_ITEMS WHERE LOGICALREF=@ID", conn, tran)
                    { Parameters = { new SqlParameter("@ID", logicalRef) } }.ExecuteNonQuery();

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }



    }
}
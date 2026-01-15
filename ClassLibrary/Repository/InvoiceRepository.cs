using Library.Data;
using Library.Models.Order;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Models.Invoice;

namespace Library.Repository
{
    public class InvoiceRepository
    {
        // ORDERS LIST
        public List<InvoiceHeaderModel> GetInvoices()
        {
            var list = new List<InvoiceHeaderModel>();

            DataTable dt = SqlHelper.Select(@"
                SELECT TOP 1000
                    I.LOGICALREF,
                    I.FICHENO,
                    I.DATE_,
                    I.TIME_,
                    I.CLIENTREF,
                    C.DEFINITION_ AS CLIENTNAME,
                    I.NETTOTAL
                FROM LG_001_01_INVOICE I
                LEFT JOIN LG_001_CLCARD C ON C.LOGICALREF = I.CLIENTREF
                ORDER BY I.LOGICALREF DESC
            ");

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new InvoiceHeaderModel
                {
                
                    LOGICALREF = row["LOGICALREF"] == DBNull.Value ? 0 :
                        Convert.ToInt32(row["LOGICALREF"]),
                    FICHENO = row["FICHENO"]?.ToString() ?? "",
                    DATE_ = row["DATE_"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(row["DATE_"]),
                    TIME_ = row["TIME_"] == DBNull.Value ? 0 :
                        Convert.ToInt32(row["TIME_"]),
                    CLIENTREF = row["CLIENTREF"] == DBNull.Value ? 0 :
                        Convert.ToInt32(row["CLIENTREF"]),
                    CLIENTNAME = row["CLIENTNAME"]?.ToString() ?? "",
                    NETTOTAL = row["NETTOTAL"] == DBNull.Value ? 0 :
                        Convert.ToDecimal(row["NETTOTAL"])


                });
            }

            return list;
        }





        public bool DeleteInvoice(int logicalRef)
        {
            using SqlConnection con = new SqlConnection(SqlHelper.connStr);
            con.Open();

            using SqlTransaction tran = con.BeginTransaction();
            try
            {
                SqlHelper.Execute(
                    "DELETE FROM LG_001_01_STLINE WHERE INVOICEREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", logicalRef));

                int rows = SqlHelper.Execute(
                    "DELETE FROM LG_001_01_INVOICE WHERE LOGICALREF = @REF",
                    con, tran,
                    new SqlParameter("@REF", logicalRef));

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

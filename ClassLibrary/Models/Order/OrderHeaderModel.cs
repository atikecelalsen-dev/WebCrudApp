
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Library.Models.Order
{
    public class OrderHeaderModel
    {
            public int LOGICALREF { get; set; }
            public int TRCODE { get; set; }
            public string? FICHENO { get; set; }
            public DateTime DATE_ { get; set; }
            public int TIME_ { get; set; }
            public int CLIENTREF { get; set; }

            public string? CLIENTNAME { get; set; }

            public decimal GROSSTOTAL { get; set; }
            public decimal TOTALVAT { get; set; }
            public decimal NETTOTAL { get; set; }
            public decimal REPORTNET { get; set; }
            public decimal REPORTRATE { get; set; }
            public decimal ADDDISCOUNTS { get; set; }
            public decimal TOTALDISCOUNTS { get; set; }
            public decimal TOTALDISCOUNTED { get; set; }
       
     
    

            public short BRANCH { get; set; }
            public short DEPARTMENT { get; set; }

            public short STATUS { get; set; }
            public short RECSTATUS { get; set; }
            public short CANCELLED { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models.Invoice
{
    public class InvoiceHeaderModel
    {
        // GRPCODE, TRCODE, FICHENO, DATE_, TIME_, CLIENTREF,
        // VAT, ADDDISCOUNTS, TOTALDISCOUNTS, TOTALDISCOUNTED, TOTALVAT, GROSSTOTAL,
        // NETTOTAL,REPORTRATE, REPORTNET,
        // BRANCH, DEPARTMENT ,RECSTATUS, STATUS, CANCELLED

        //TRCURR, TRRATE, TRNET,  KASTRANSREF,  PAIDINCASH, FROMKASA, ENTEGSET,
        // AFFECTRISK, ESTATUS, TOTALSERVICES, ORDFICHECMREF, COSFCREFINFL, 
      

        public int LOGICALREF { get; set; }
        public int GRPCODE { get; set; }
        public int TRCODE { get; set; }
        public string? FICHENO { get; set; }
        public DateTime DATE_ { get; set; }
        public int TIME_ { get; set; }
        public int CLIENTREF { get; set; }

        public string? CLIENTNAME { get; set; }

        public decimal VAT { get; set; }
        public decimal ADDDISCOUNTS { get; set; }
        public decimal TOTALDISCOUNTS { get; set; }
        public decimal TOTALDISCOUNTED { get; set; }
        public decimal TOTALVAT { get; set; }
        public decimal GROSSTOTAL { get; set; }
        
        public decimal NETTOTAL { get; set; }
        public decimal REPORTNET { get; set; }
        public decimal REPORTRATE { get; set; }

        public short BRANCH { get; set; }
        public short DEPARTMENT { get; set; }

        public short STATUS { get; set; }
        public short RECSTATUS { get; set; }
        public short CANCELLED { get; set; }
        public short ORDFICHECMREF { get; set; }

        public int InvoiceType { get; set; } // 1 = Satın Alma, 2 = Satış

        public string InvoiceTypeName => InvoiceType switch
        {
            1 => "Satın Alma Faturası",
            2 => "Satış Faturası",
            _ => "Bilinmiyor"
        };

    }
}

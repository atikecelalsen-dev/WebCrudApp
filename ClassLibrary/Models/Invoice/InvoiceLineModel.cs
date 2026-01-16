using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models.Invoice
{
    public class InvoiceLineModel
    {

        // LOGICALREF, INVOICEREF, INVOICELNNO, STOCKREF, CLIENTREF,
        // LINETYPE, DETLINE, TRCODE, DATE_, FTIME, AMOUNT, PRICE, TOTAL, 
        // VAT, VATAMNT, VATMATRAH,  LINENET, UOMREF, USREF,
        // UINFO1, UINFO2, GROSSUINFO1, GROSSUINFO2,
        // DORESERVE, RECSTATUS, CANCELLED, STATUS

        //PREVLINEREF, PREVLINENO,IOCODE, STFICHEREF, STFICHELNNO, 
        //ORDTRANSREF, ORDFICHEREF, PRCURR, PRPRICE, TRCURR, TRRATE,
        //REPORTRATE, DISTCOST, DISTDISC,   
        //STFICHEREF, STFICHELLNO, BILLEDITEM, BILLED, MONTH_, YEAR_

        public int LOGICALREF { get; set; }
        public int INVOICEREF { get; set; }
        public int INVOICELNNO { get; set; }

        public int STOCKREF { get; set; }
        public int CLIENTREF { get; set; }

        public short? LINETYPE { get; set; }
       
        public short DETLINE { get; set; }
        public int TRCODE { get; set; }

        public DateTime DATE_ { get; set; }
        public int FTIME { get; set; }

        public decimal AMOUNT { get; set; }
        public decimal PRICE { get; set; }
        public decimal TOTAL { get; set; }

        public int VAT { get; set; }
        public decimal VATAMNT { get; set; }
        public decimal VATMATRAH { get; set; }
        public decimal LINENET { get; set; }

        public int UOMREF { get; set; }
        public int USREF { get; set; }

        public decimal UINFO1 { get; set; }
        public decimal UINFO2 { get; set; }
        public decimal GROSSUINFO1 { get; set; }
        public decimal GROSSUINFO2 { get; set; }

        public short DORESERVE { get; set; }
        public short RECSTATUS { get; set; }
        public short CANCELLED { get; set; }
        public short STATUS { get; set; }

        public List<SelectListItem> Units { get; set; } = new List<SelectListItem>();
        
    }
}

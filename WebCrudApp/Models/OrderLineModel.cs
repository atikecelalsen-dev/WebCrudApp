using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCrudApp.Models
{
    public class OrderLineModel
    {
        public int LOGICALREF { get; set; }
        public int ORDFICHEREF { get; set; }

        public int STOCKREF { get; set; }
        public int CLIENTREF { get; set; }

        public short LINETYPE { get; set; }
        public short LINENO_ { get; set; }
        public short DETLINE { get; set; }
        public int TRCODE { get; set; }
        public DateTime DATE_ { get; set; }
        public int TIME_ { get; set; }

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
        public short SHIPPEDAMOUNT { get; set; }
        public short CLOSED { get; set; }
        public short DORESERVE { get; set; }
        public short RECSTATUS { get; set; }
        public short CANCELLED { get; set; }

        public List<SelectListItem> Units { get; set; } = new List<SelectListItem>();

    }
}

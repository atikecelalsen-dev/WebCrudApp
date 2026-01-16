namespace Library.Models.Invoice
{
    public class InvoiceItemViewModel
    {
        public string Value { get; set; }          // LOGICALREF
        public string Code { get; set; }           // MalzemeKodu
        public string Text { get; set; }           // MalzemeAdi
        public decimal OnHand { get; set; }
    }
}

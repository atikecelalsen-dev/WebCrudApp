namespace Library.Models.Order
{
    public class OrderItemViewModel
    {
        public string Value { get; set; }          // LOGICALREF
        public string Code { get; set; }           // MalzemeKodu
        public string Text { get; set; }           // MalzemeAdi
        public decimal OnHand { get; set; }

    }
}

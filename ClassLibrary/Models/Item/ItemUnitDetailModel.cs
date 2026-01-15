namespace Library.Models.Item
{
    public class ItemUnitDetailModel
    {
        public string? ITEMNAME { get; set; }
        public int? LOGICALREF { get; set; }
        public int? ITMUNITAREF { get; set; }
        public int? ITEMREF { get; set; }
        public int? UNITLINEREF { get; set; }
        public string? UNITNAME { get; set; }

        public string? CODE { get; set; }
        public string? BARCODE { get; set; }

        public decimal? PURCHASEPRICE { get; set; }
        public decimal? SALEPRICE { get; set; }
    }
}

namespace WebCrudApp.Models
{
    public class ItemPageViewModel
    {
        // Form için
        public ItemViewModel Item { get; set; }

       // public ItemUnitModel Unit {{ get; set; } }

        // Grid için
        public List<ItemListModel> ItemList { get; set; }
    }
}

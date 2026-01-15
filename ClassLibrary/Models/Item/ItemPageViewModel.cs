using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.Models.Item
{
    public class ItemPageViewModel
    {

        public ItemViewModel Item { get; set; }
        public List<SelectListItem> UnitSets { get; set; }
        public List<ItemViewModel> ItemList { get; set; }

        public string SearchCode { get; set; }
        public string SearchName { get; set; }
        public int? SearchUnitSetRef { get; set; }
    }
}

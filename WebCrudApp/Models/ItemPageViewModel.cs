using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCrudApp.Models
{
    public class ItemPageViewModel
    {
        public ItemViewModel Item { get; set; }
        public List<SelectListItem> UnitSets { get; set; }
        public List<ItemViewModel> ItemList { get; set; }
    }
}

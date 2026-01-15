using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Library.Models.Item
{
    public class ItemViewModel
    {
        [JsonPropertyName("logicalRef")]
        public int LOGICALREF { get; set; }

        [JsonPropertyName("code")]
        public string CODE { get; set; }

        [JsonPropertyName("name")]
        public string NAME { get; set; }
        public int ACTIVE { get; set; }
        public int CARDTYPE { get; set; }

        public int CLASSTYPE { get; set; }
        public int UNITSETREF { get; set; }
        public string UNITNAME { get; set; }

        public List<ItemUnitDetailModel> UnitDetails { get; set; }

    } 
}



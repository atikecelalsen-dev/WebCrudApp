using System.Text.Json.Serialization;

namespace 
    Library.Models.Client
{
    public class ClientAJAXModel
    {
        [JsonPropertyName("logicalRef")]
        public int LOGICALREF { get; set; }

        [JsonPropertyName("code")]
        public string? CODE { get; set; }

        [JsonPropertyName("definition")]
        public string? DEFINITION_ { get; set; }

        public int ACTIVE { get; set; }
        public int CARDTYPE { get; set; }
    }
}


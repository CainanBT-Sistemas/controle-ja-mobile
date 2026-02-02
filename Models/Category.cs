using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    public class Category
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryType")] public TransactionType Type { get; set; }

        [JsonPropertyName("icon")] public string Icon { get; set; } = "";

        public string Color { get; set; }
    }
}

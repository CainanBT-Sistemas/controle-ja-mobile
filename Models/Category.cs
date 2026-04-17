using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.Models
{
    public partial class Category : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryType")]
        public TransactionType Type { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "category";

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#94A3B8";

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("parentId")]
        public Guid? ParentId { get; set; }

        [JsonIgnore]
        public ObservableCollection<Category> SubCategories { get; set; } = new();

        [ObservableProperty]
        [JsonIgnore]
        private bool isExpanded = false;

        [JsonIgnore]
        public bool HasChildren => SubCategories != null && SubCategories.Count > 0;

        [JsonIgnore]
        public int ChildrenCount => SubCategories?.Count ?? 0;

        [JsonIgnore]
        public string ParentColor { get; set; }
    }
}
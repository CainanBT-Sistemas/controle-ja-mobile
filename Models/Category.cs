using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class CategoryGroup : ObservableObject
    {
        public string Name { get; private set; }
        public string TypeDescription { get; private set; } // Ex: "Entradas" ou "Saídas"

        [ObservableProperty]
        private bool isExpanded;

        // Lista completa (Banco de dados)
        private List<Category> _allItems;

        // Lista que aparece na tela (a gente adiciona/remove itens daqui para criar o efeito)
        public ObservableCollection<Category> VisibleItems { get; } = new();

        public CategoryGroup(string name, string typeDesc, List<Category> items)
        {
            Name = name;
            TypeDescription = typeDesc;
            _allItems = items;
            IsExpanded = true; // Começa aberto
            UpdateVisibleItems();
        }

        public void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
            UpdateVisibleItems();
        }

        private void UpdateVisibleItems()
        {
            VisibleItems.Clear();
            if (IsExpanded)
            {
                foreach (var item in _allItems) VisibleItems.Add(item);
            }
        }
    }
}

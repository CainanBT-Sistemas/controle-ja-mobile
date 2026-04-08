using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.Models
{
    public partial class SelectionItem : ObservableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        // A mágica: guarda o objeto original (Categoria, Conta, etc) para devolver intacto
        public object OriginalObject { get; set; }

        public bool IsSubItem { get; set; }
        public bool HasSubItems => SubItems?.Any() == true;

        public ObservableCollection<SelectionItem> SubItems { get; set; } = new();

        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BgColor))]
        [NotifyPropertyChangedFor(nameof(CheckColor))]
        private bool isSelected;

        public string BgColor => IsSelected ? "#1E293B" : "Transparent";
        public string CheckColor => IsSelected ? "#00E676" : "#334155";
    }
}
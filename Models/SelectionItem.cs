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
        public string ParentColor { get; set; }
        public object OriginalObject { get; set; }
        public bool IsSubItem { get; set; }

        public ObservableCollection<SelectionItem> SubItems { get; set; } = new();
        public bool HasSubItems => SubItems?.Any() == true;

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isExpanded = true;

        public string BgColor => IsSelected ? "#1E293B" : "Transparent";
    }
}
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class ItemSelectionViewModel : BaseViewModel
    {
        private readonly CategoryService _categoryService;
        private TransactionType? _currentTypeFilter;

        public Popup? PopupInstance { get; set; }

        [ObservableProperty] private string pageTitle = "Selecione";
        public ObservableCollection<SelectionItem> Items { get; } = new();
        [ObservableProperty] private SelectionItem currentSelection;
        [ObservableProperty] private bool isCategorySelection;

        public ItemSelectionViewModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // NOVO: Usado para inicializar os dados pelo Popup ao invés da navegação antiga
        public void Initialize(string title, IEnumerable<SelectionItem> items)
        {
            PageTitle = title;
            IsCategorySelection = title == "Categoria";
            Items.Clear();
            foreach (var item in items) Items.Add(item);

            var firstCat = items.FirstOrDefault()?.OriginalObject as Category;
            if (firstCat != null) _currentTypeFilter = firstCat.Type;
        }

        [RelayCommand]
        public async Task ReloadCategories()
        {
            if (!IsCategorySelection || _currentTypeFilter == null) return;
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var cats = await _categoryService.GetCategoriesAsync();
                if (cats == null) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                    var list = cats
                        .Where(c => c.ParentId == null && c.Type == _currentTypeFilter)
                        .OrderBy(c => c.Name)
                        .Select(cat =>
                        {
                            var nivel1 = new SelectionItem { Id = cat.Id, Name = cat.Name, Icon = cat.Icon, Color = cat.Color, OriginalObject = cat };
                            var filhos = cats.Where(x => x.ParentId == cat.Id).OrderBy(s => s.Name).ToList();
                            foreach (var sub in filhos)
                            {
                                nivel1.SubItems.Add(new SelectionItem
                                {
                                    Id = sub.Id,
                                    Name = sub.Name,
                                    Icon = sub.Icon,
                                    Color = sub.Color,
                                    ParentColor = cat.Color,
                                    OriginalObject = sub,
                                    IsSubItem = true
                                });
                            }
                            return nivel1;
                        }).ToList();

                    foreach (var item in list) Items.Add(item);
                });
            }, showLoading: false);
        }

        [RelayCommand]
        public async Task GoToManageCategories()
        {
            PopupInstance?.Close();
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                await Task.Delay(50);
                await Shell.Current.GoToAsync("ManageCategoriesPage");
            });
        }

        [RelayCommand]
        public void ToggleExpand(SelectionItem item)
        {
            if (item.HasSubItems) item.IsExpanded = !item.IsExpanded;
        }

        [RelayCommand]
        public void SelectItem(SelectionItem item)
        {
            foreach (var i in Items)
            {
                i.IsSelected = false;
                foreach (var sub in i.SubItems) sub.IsSelected = false;
            }
            item.IsSelected = true;
            CurrentSelection = item;
        }

        [RelayCommand]
        public void ConfirmSelection()
        {
            if (CurrentSelection == null) { App.Current.MainPage.DisplayAlert("Aviso", "Selecione um item.", "OK"); return; }
            WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(CurrentSelection.OriginalObject));
            PopupInstance?.Close();
        }

        [RelayCommand]
        public void Close() => PopupInstance?.Close();
    }
}
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public Popup? PopupInstance { get; set; }

        public ObservableCollection<Category> DisplayedCategories { get; } = new();

        private List<Category> _allExpenses = new();
        private List<Category> _allIncomes = new();

        [ObservableProperty]
        private bool isShowingExpenses = true;

        [ObservableProperty]
        private bool isShowingIncomes = false;

        [ObservableProperty]
        private bool isRefreshing;

        public CategoriesViewModel(ApiService apiService)
        {
            _apiService = apiService;

            // MÁGICA: Escuta quando alguém salva/deleta uma categoria e recarrega a lista sozinho!
            WeakReferenceMessenger.Default.Register<GlobalRefreshMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _ = LoadCategories();
                });
            });
        }

        [RelayCommand]
        public void ShowExpenses()
        {
            IsShowingExpenses = true;
            IsShowingIncomes = false;
            UpdateDisplayedList();
        }

        [RelayCommand]
        public void ShowIncomes()
        {
            IsShowingExpenses = false;
            IsShowingIncomes = true;
            UpdateDisplayedList();
        }

        private void UpdateDisplayedList()
        {
            DisplayedCategories.Clear();
            var targetList = IsShowingExpenses ? _allExpenses : _allIncomes;

            if (targetList != null)
            {
                foreach (var item in targetList)
                {
                    DisplayedCategories.Add(item);
                }
            }
        }

        [RelayCommand]
        public async Task LoadCategories()
        {
            IsRefreshing = true;
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                string jsonString = await _apiService.GetAsync<string>("categories");
                List<Category> fullList = new();

                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    fullList = JsonSerializer.Deserialize<List<Category>>(jsonString, options) ?? new();
                }

                var rootCategories = fullList
                    .Where(c => c.ParentId == null)
                    .OrderBy(c => c.Name)
                    .ToList();

                foreach (var root in rootCategories)
                {
                    root.SubCategories.Clear();

                    var filhos = fullList
                        .Where(c => c.ParentId == root.Id)
                        .OrderBy(f => f.Name)
                        .ToList();

                    foreach (var sub in filhos)
                    {
                        sub.ParentColor = root.Color;
                        root.SubCategories.Add(sub);
                    }
                }

                _allExpenses = rootCategories.Where(c => c.Type == TransactionType.DESPESA).ToList();
                _allIncomes = rootCategories.Where(c => c.Type == TransactionType.RECEITA).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateDisplayedList();
                });
            });

            IsRefreshing = false;
        }

        private void ShowCategoryPopup(string? id = null, string? parentType = null)
        {
            var vm = IPlatformApplication.Current?.Services.GetService<CategoryAddViewModel>();
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(id)) vm.CategoryId = id;
                if (!string.IsNullOrEmpty(parentType)) vm.ParentType = parentType;

                var popup = new Views.Popups.CategoryAddPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void AddCategory()
        {
            string typeStr = IsShowingExpenses ? "DESPESA" : "RECEITA";
            ShowCategoryPopup(null, typeStr);
        }

        [RelayCommand]
        public void OpenCategoryDetails(Category category)
        {
            if (category == null) return;
            ShowCategoryPopup(category.Id.ToString(), null);
        }

        [RelayCommand]
        public void GoBack() => PopupInstance?.Close();
    }
}
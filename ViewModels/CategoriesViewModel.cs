using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Category> DisplayedCategories { get; } = new();

        private List<Category> _allExpenses = new();
        private List<Category> _allIncomes = new();

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private bool isExpensesSelected = true;

        public CategoriesViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadCategories()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                string jsonString = await _apiService.GetAsync<string>("categories");

                List<Category> fullList = new();

                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        options.Converters.Add(new JsonStringEnumConverter());
                        fullList = JsonSerializer.Deserialize<List<Category>>(jsonString, options) ?? new();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERRO JSON: {ex.Message}");
                    }
                }

                // MONTAGEM DA ÁRVORE
                var rootCategories = fullList.Where(c => c.ParentId == null).ToList();
                var subCategories = fullList.Where(c => c.ParentId != null).ToList();

                foreach (var root in rootCategories) root.SubCategories.Clear();

                foreach (var sub in subCategories)
                {
                    var parent = rootCategories.FirstOrDefault(p => p.Id == sub.ParentId);
                    if (parent != null)
                    {
                        parent.SubCategories.Add(sub);
                    }
                }

                _allExpenses = rootCategories.Where(c => c.Type == TransactionType.DESPESA).ToList();
                _allIncomes = rootCategories.Where(c => c.Type == TransactionType.RECEITA).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateDisplayedList();
                });

                IsRefreshing = false;
            });
        }

        [RelayCommand]
        public void SwitchTab(string type)
        {
            IsExpensesSelected = type == "DESPESA";
            UpdateDisplayedList();
        }

        private void UpdateDisplayedList()
        {
            DisplayedCategories.Clear();
            var source = IsExpensesSelected ? _allExpenses : _allIncomes;

            // EXIBE APENAS AS RAÍZES
            foreach (var item in source.Where(c => c.ParentId == null))
            {
                DisplayedCategories.Add(item);
            }
        }

        [RelayCommand]
        public async Task OpenCategoryDetails(Category category)
        {
            await Shell.Current.GoToAsync($"{nameof(CategoryAddPage)}?id={category.Id}");
        }

        [RelayCommand]
        public async Task GoToAddRootCategory()
        {
            await Shell.Current.GoToAsync(nameof(CategoryAddPage));
        }

        [RelayCommand]
        public async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}
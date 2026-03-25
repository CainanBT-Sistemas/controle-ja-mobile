using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.ViewModels
{
    [QueryProperty(nameof(ParentCategoryId), "parentId")]
    [QueryProperty(nameof(ParentType), "parentType")]
    [QueryProperty(nameof(CategoryId), "id")]
    public partial class CategoryAddViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private string title = "Nova Categoria";
        [ObservableProperty] private string name;
        [ObservableProperty] private string categoryId;
        [ObservableProperty] private string parentCategoryId;
        [ObservableProperty] private string parentType;

        [ObservableProperty] private bool isEditMode = false;

        public List<string> TransactionTypes { get; } = new() { "Despesa", "Receita" };

        [ObservableProperty] private string selectedTransactionType = "Despesa";
        [ObservableProperty] private bool isTypeSelectionEnabled = true;
        [ObservableProperty] private string selectedIcon;
        [ObservableProperty] private string selectedColor;
        [ObservableProperty] private bool isColorSelectorOpen;
        [ObservableProperty] private bool isIconSelectorOpen;

        public ObservableCollection<Category> SubCategories { get; } = new();

        public ObservableCollection<string> AvailableIcons { get; } = new(UIConstants.AvailableIcons);
        public ObservableCollection<string> AvailableColors { get; } = new(UIConstants.AvailableColors);

        public CategoryAddViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SelectedIcon = AvailableIcons.First();
            SelectedColor = AvailableColors.First();
        }

        partial void OnParentCategoryIdChanged(string value) => ApplySubCategoryLogic();
        partial void OnParentTypeChanged(string value) => ApplySubCategoryLogic();

        private void ApplySubCategoryLogic()
        {
            if (!string.IsNullOrEmpty(ParentCategoryId))
            {
                IsTypeSelectionEnabled = false;
                if (!string.IsNullOrEmpty(ParentType))
                    SelectedTransactionType = ParentType.ToUpper() == "RECEITA" ? "Receita" : "Despesa";
            }
        }

        partial void OnCategoryIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Title = "Editar Categoria";
                IsEditMode = true;
                _ = LoadCategoryData(value);
            }
        }

        private async Task LoadCategoryData(string id)
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var json = await _apiService.GetAsync<string>($"categories/{id}");
                var allCategoriesJson = await _apiService.GetAsync<string>("categories");

                if (!string.IsNullOrEmpty(json) && !string.IsNullOrEmpty(allCategoriesJson))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());

                    var category = JsonSerializer.Deserialize<Category>(json, options);
                    var fullList = JsonSerializer.Deserialize<List<Category>>(allCategoriesJson, options) ?? new();

                    foreach (var sub in fullList.Where(c => c.ParentId != null))
                    {
                        var parent = fullList.FirstOrDefault(p => p.Id == sub.ParentId);
                        parent?.SubCategories.Add(sub);
                    }

                    if (category != null)
                    {
                        var myChildren = fullList.Where(c => c.ParentId == category.Id).ToList();

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Name = category.Name;
                            SelectedColor = category.Color;
                            SelectedIcon = category.Icon;
                            SelectedTransactionType = category.Type == TransactionType.RECEITA ? "Receita" : "Despesa";

                            if (category.IsDefault) IsTypeSelectionEnabled = false;

                            SubCategories.Clear();
                            foreach (var child in myChildren)
                            {
                                SubCategories.Add(child);
                            }
                        });
                    }
                }
            });
        }

        [RelayCommand] private void OpenColorSelector() => IsColorSelectorOpen = true;
        [RelayCommand] private void CloseColorSelector() => IsColorSelectorOpen = false;
        [RelayCommand] private void OpenIconSelector() => IsIconSelectorOpen = true;
        [RelayCommand] private void CloseIconSelector() => IsIconSelectorOpen = false;
        [RelayCommand] private void SelectIcon(string icon) { SelectedIcon = icon; IsIconSelectorOpen = false; }
        [RelayCommand] private void SelectColor(string color) { SelectedColor = color; IsColorSelectorOpen = false; }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Aviso", "Por favor, digite o nome.", "OK");
                return;
            }

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var dto = new
                {
                    name = Name.Trim(),
                    categoryType = SelectedTransactionType == "Receita" ? "RECEITA" : "DESPESA",
                    icon = SelectedIcon,
                    color = SelectedColor,
                    parentId = string.IsNullOrEmpty(ParentCategoryId) ? null : ParentCategoryId
                };

                string result;
                if (string.IsNullOrEmpty(CategoryId))
                    result = await _apiService.PostAsync<object>("categories", dto);
                else
                    result = await _apiService.PutAsync<object>($"categories/{CategoryId}", dto);

                if (!string.IsNullOrEmpty(result))
                    await Shell.Current.GoToAsync("..");
            });
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool confirm = await Shell.Current.DisplayAlert("Excluir", $"Deseja apagar a categoria '{Name}'?", "Sim", "Não");
            if (!confirm) return;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var result = await _apiService.DeleteAsync($"categories/{CategoryId}");
                if (!string.IsNullOrEmpty(result))
                {
                    await Shell.Current.GoToAsync("..");
                }
            });
        }

        [RelayCommand]
        public async Task GoToAddSubCategory()
        {
            string typeStr = SelectedTransactionType == "Receita" ? "RECEITA" : "DESPESA";
            await Shell.Current.GoToAsync($"{nameof(CategoryAddPage)}?parentId={CategoryId}&parentType={typeStr}");
        }

        [RelayCommand]
        public async Task OpenCategoryDetails(Category category)
        {
            await Shell.Current.GoToAsync($"{nameof(CategoryAddPage)}?id={category.Id}");
        }

        [RelayCommand]
        private async Task GoBack() => await Shell.Current.GoToAsync("..");

        // --- NOVO: MÉTODO QUE RECARREGA APENAS A LISTA DE FILHAS ---
        public async Task ReloadSubCategoriesAsync()
        {
            if (!IsEditMode || string.IsNullOrEmpty(CategoryId)) return;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var allCategoriesJson = await _apiService.GetAsync<string>("categories");

                if (!string.IsNullOrEmpty(allCategoriesJson))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var fullList = JsonSerializer.Deserialize<List<Category>>(allCategoriesJson, options) ?? new();

                    // Conecta as categorias para o "Badge" (contador) atualizar também
                    foreach (var sub in fullList.Where(c => c.ParentId != null))
                    {
                        var parent = fullList.FirstOrDefault(p => p.Id == sub.ParentId);
                        parent?.SubCategories.Add(sub);
                    }

                    if (Guid.TryParse(CategoryId, out Guid currentParentId))
                    {
                        var myChildren = fullList.Where(c => c.ParentId == currentParentId).ToList();

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            SubCategories.Clear();
                            foreach (var child in myChildren)
                            {
                                SubCategories.Add(child);
                            }
                        });
                    }
                }
            });
        }
    }
}
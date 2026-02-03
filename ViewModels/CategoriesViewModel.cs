using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace controle_ja_mobile.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        // Lista de Grupos (Pai)
        public ObservableCollection<CategoryGroup> CategoryGroups { get; } = new();

        [ObservableProperty] private bool isRefreshing;
        [ObservableProperty] private bool isEmptyStateVisible;

        public CategoriesViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadCategories()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                // 1. Busca o JSON puro como string (já que seu método retorna string por padrão)
                string jsonString = await _apiService.GetAsync<string>("categories");

                List<Category> categoriesList = new List<Category>();

                // 2. Converte (Deserializa) a String para Lista de Categorias
                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        categoriesList = System.Text.Json.JsonSerializer.Deserialize<List<Category>>(jsonString, options)
                                         ?? new List<Category>();
                    }
                    catch
                    {
                        // Se o JSON vier inválido, mantemos a lista vazia para não quebrar a tela
                        categoriesList = new List<Category>();
                    }
                }

                // 3. Limpa e popula os grupos visuais
                CategoryGroups.Clear();

                if (categoriesList != null && categoriesList.Any())
                {
                    IsEmptyStateVisible = false;

                    // Agora 'categoriesList' é uma Lista real, então o .Where funciona corretamente
                    var receitas = categoriesList.Where(c => c.Type == TransactionType.RECEITA).ToList();
                    var despesas = categoriesList.Where(c => c.Type == TransactionType.DESPESA).ToList();

                    if (receitas.Any())
                        CategoryGroups.Add(new CategoryGroup("Receitas", "Entradas", receitas));

                    if (despesas.Any())
                        CategoryGroups.Add(new CategoryGroup("Despesas", "Saídas", despesas));
                }
                else
                {
                    IsEmptyStateVisible = true;
                }

                IsRefreshing = false;
            });
        }

        // Comando para abrir/fechar o grupo
        [RelayCommand]
        public void ToggleGroup(CategoryGroup group)
        {
            group.ToggleExpand();
        }

        [RelayCommand]
        public async Task EditCategory(Category category)
        {
            await Shell.Current.DisplayAlert("Editar", $"Editar {category.Name}", "OK");
        }

        [RelayCommand]
        public async Task DeleteCategory(Category category)
        {
            bool confirm = await Shell.Current.DisplayAlert("Excluir", $"Apagar {category.Name}?", "Sim", "Não");
            if (!confirm) return;

            await ExecuteWithErrorHandlingAsync(async () => {
                var success = await _apiService.DeleteAsync($"categories/{category.Id}");
                if (!string.IsNullOrWhiteSpace(success)) await LoadCategories();
            });
        }

        [RelayCommand]
        public async Task GoBack() => await Shell.Current.GoToAsync("..");

        [RelayCommand]
        public async Task GoToAddCategory() => await Shell.Current.DisplayAlert("Adicionar", "Em breve", "OK");
    }
}
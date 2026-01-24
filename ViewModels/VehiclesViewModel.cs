using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class VehiclesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        // --- Propriedades para a LISTAGEM ---
        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing; // Controla o "puxar para atualizar"

        // --- Propriedades para o CADASTRO (Formulário) ---
        [ObservableProperty] private string newName;
        [ObservableProperty] private string newBrand;
        [ObservableProperty] private string newModel;
        [ObservableProperty] private string newYear;
        [ObservableProperty] private string newPlate;
        [ObservableProperty] private string newOdometer;

        // Construtor com Injeção de Dependência
        public VehiclesViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // COMANDO: Carregar Lista de Veículos
        [RelayCommand]
        public async Task LoadVehicles()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                // Chama a API: GET /vehicles
                var list = await _apiService.GetAsync<List<Vehicle>>("vehicles");

                Vehicles.Clear();
                if (list != null)
                {
                    // Adiciona na lista observável para aparecer na tela
                    foreach (var v in list)
                    {
                        Vehicles.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar veículos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false; // Para a animação do refresh
            }
        }

        // COMANDO: Ir para tela de Adicionar
        [RelayCommand]
        public async Task GoToAddPage()
        {
            // Navega para a página de cadastro
            // O nome "VehicleAddPage" deve ter sido registrado no AppShell.xaml.cs (Routing.RegisterRoute)
            await Shell.Current.GoToAsync(nameof(VehicleAddPage));
        }

        // COMANDO: Salvar Novo Veículo
        [RelayCommand]
        public async Task SaveVehicle()
        {
            // 1. Validação Básica
            if (string.IsNullOrWhiteSpace(NewName) ||
                string.IsNullOrWhiteSpace(NewModel) ||
                string.IsNullOrWhiteSpace(NewOdometer))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha pelo menos Nome, Modelo e KM Atual.", "OK");
                return;
            }

            IsLoading = true;

            try
            {
                // 2. Monta o objeto anônimo igual ao DTO que o Java espera
                var newVehicleData = new
                {
                    name = NewName,
                    brand = NewBrand ?? "", // Evita null
                    model = NewModel,
                    year = int.TryParse(NewYear, out int y) ? y : 2024, // Tenta converter ano, senão usa 2024
                    plate = NewPlate ?? "",
                    currentOdometer = decimal.Parse(NewOdometer) // Converte string para decimal
                };

                // 3. Envia para a API: POST /vehicles
                var result = await _apiService.PostAsync<Vehicle>("vehicles", newVehicleData);

                if (result != null)
                {
                    await App.Current.MainPage.DisplayAlert("Sucesso", "Veículo cadastrado!", "OK");

                    // 4. Limpa os campos
                    NewName = string.Empty;
                    NewBrand = string.Empty;
                    NewModel = string.Empty;
                    NewYear = string.Empty;
                    NewPlate = string.Empty;
                    NewOdometer = string.Empty;

                    // 5. Volta para a lista
                    await Shell.Current.GoToAsync("..");

                    // 6. Atualiza a lista para mostrar o novo carro
                    await LoadVehicles();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Falha ao salvar veículo.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

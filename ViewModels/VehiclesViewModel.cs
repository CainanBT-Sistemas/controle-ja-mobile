using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class VehiclesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        // Campos do Formulário
        [ObservableProperty] private string newName;
        [ObservableProperty] private string newBrand;
        [ObservableProperty] private string newModel;
        [ObservableProperty] private string newYear;
        [ObservableProperty] private string newPlate;
        [ObservableProperty] private string newOdometer;

        public VehiclesViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadVehicles()
        {
            await ExecuteAsync(async () =>
            {
                try
                {
                    var list = await _apiService.GetAsync<List<Vehicle>>("vehicles");
                    Vehicles.Clear();
                    if (list != null)
                    {
                        foreach (var v in list) Vehicles.Add(v);
                    }
                }
                finally
                {
                    IsRefreshing = false;
                }
            }, showLoading: false);
        }

        [RelayCommand]
        public async Task GoToAddPage()
        {
            await Shell.Current.GoToAsync(nameof(VehicleAddPage));
        }

        [RelayCommand]
        public async Task SaveVehicle()
        {
            if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewModel) || string.IsNullOrWhiteSpace(NewOdometer))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha pelo menos Nome, Modelo e KM Atual.", "OK");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var newVehicleData = new
                {
                    name = NewName,
                    brand = NewBrand ?? "",
                    model = NewModel,
                    year = int.TryParse(NewYear, out int y) ? y : 2024,
                    plate = NewPlate ?? "",
                    currentOdometer = decimal.Parse(NewOdometer)
                };

                var result = await _apiService.PostAsync<Vehicle>("vehicles", newVehicleData);

                if (result != null)
                {
                    await App.Current.MainPage.DisplayAlert("Sucesso", "Veículo cadastrado!", "OK");

                    // Limpar
                    NewName = string.Empty;
                    NewBrand = string.Empty;
                    NewModel = string.Empty;
                    NewYear = string.Empty;
                    NewPlate = string.Empty;
                    NewOdometer = string.Empty;

                    await Shell.Current.GoToAsync("..");
                    await LoadVehicles();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Falha ao salvar veículo.", "OK");
                }
            });
        }
    }
}
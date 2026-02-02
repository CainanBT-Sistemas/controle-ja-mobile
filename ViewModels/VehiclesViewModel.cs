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
       
        private readonly VehicleService _vehicleService;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        // Campos do Formulário
        [ObservableProperty] private string newName;
        [ObservableProperty] private string newBrand;
        [ObservableProperty] private string newModel;
        [ObservableProperty] private string newYear;
        [ObservableProperty] private string newPlate;
        [ObservableProperty] private string newOdometer;

        public VehiclesViewModel(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [RelayCommand]
        public async Task LoadVehicles()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                try
                {
                    var list = await _vehicleService.GetVehiclesAsync();
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

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                Vehicle newVehicleData = new Vehicle();
                newVehicleData.Name = NewName;
                newVehicleData.Model = NewModel;
                newVehicleData.Brand = NewBrand;
                newVehicleData.Year = int.TryParse(NewYear, out int y) ? y : 2024;
                newVehicleData.Plate = NewPlate;
                newVehicleData.CurrentOdometer = decimal.Parse(NewOdometer);

                var result = await _vehicleService.SaveVehicleAsync(newVehicleData);

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
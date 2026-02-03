using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using Microcharts;
using SkiaSharp;
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
                        foreach (var v in list)
                        {
                            CalculateStatsAndChart(v);
                            Vehicles.Add(v);
                        }
                    }
                }
                finally
                {
                    IsRefreshing = false;
                }
            }, showLoading: false);
        }

        private void CalculateStatsAndChart(Vehicle vehicle)
        {
            // MOCK: Dados de custo mensal fictícios
            // (Futuramente isso virá do cálculo de abastecimentos da API)
            vehicle.MonthlyCost = "R$ 450,00";

            // Gráfico de Barras: Gastos últimos 3 meses (Simulado)
            var entries = new List<ChartEntry>
            {
                new ChartEntry(320) { Label = "Dez", ValueLabel = "320", Color = SKColor.Parse("#FFAB00") },
                new ChartEntry(450) { Label = "Jan", ValueLabel = "450", Color = SKColor.Parse("#FF5252") },
                new ChartEntry(150) { Label = "Fev", ValueLabel = "150", Color = SKColor.Parse("#00E676") }
            };

            vehicle.ExpensesChart = new BarChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 22,
                LabelColor = SKColor.Parse("#94A3B8"),
                ValueLabelOrientation = Orientation.Horizontal,
                LabelOrientation = Orientation.Horizontal
            };
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
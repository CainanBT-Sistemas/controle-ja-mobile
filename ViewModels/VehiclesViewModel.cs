using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
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

        public VehiclesViewModel(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [RelayCommand]
        public async Task LoadVehicles()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var list = await _vehicleService.GetVehiclesAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Vehicles.Clear();
                    if (list != null)
                    {
                        foreach (var v in list)
                        {
                            CalculateStatsAndChart(v);
                            Vehicles.Add(v);
                        }
                    }
                });
                IsRefreshing = false;
            });
        }

        private void CalculateStatsAndChart(Vehicle vehicle)
        {
            // MOCK: Dados de custo mensal fictícios
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
            // Ajuste o nome da View se estiver em outra pasta, assumindo que está em Views.Privates.Management
            await Shell.Current.GoToAsync(nameof(VehicleAddPage));
        }

        [RelayCommand]
        public async Task OpenVehicleDetails(Vehicle vehicle)
        {
            await Shell.Current.GoToAsync($"{nameof(VehicleAddPage)}?id={vehicle.Id}");
        }

        [RelayCommand]
        public async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}
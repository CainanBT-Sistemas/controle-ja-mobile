using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class VehiclesViewModel : BaseViewModel
    {
        private readonly VehicleService _vehicleService;

        public Popup? PopupInstance { get; set; }

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        public VehiclesViewModel(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;

            // MÁGICA: Escuta quando alguém salva/deleta um veículo e recarrega a lista sozinho!
            WeakReferenceMessenger.Default.Register<GlobalRefreshMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _ = LoadVehicles();
                });
            });
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
            vehicle.MonthlyCost = "R$ 450,00";

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

        private void ShowVehiclePopup(string? vehicleId = null)
        {
            var vm = IPlatformApplication.Current?.Services.GetService<VehicleAddViewModel>();
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(vehicleId)) vm.VehicleId = vehicleId;

                var popup = new Views.Popups.VehicleAddPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToAddPage()
        {
            ShowVehiclePopup();
        }

        [RelayCommand]
        public void OpenVehicleDetails(Vehicle vehicle)
        {
            if (vehicle != null) ShowVehiclePopup(vehicle.Id.ToString());
        }

        [RelayCommand]
        public void GoBack() => PopupInstance?.Close();
    }
}
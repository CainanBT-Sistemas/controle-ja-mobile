using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using CommunityToolkit.Maui.Views;

namespace controle_ja_mobile.ViewModels
{
    public partial class VehicleAddViewModel : BaseViewModel
    {
        private readonly VehicleService _vehicleService;
        private readonly ApiService _apiService;

        // NOVO: Referência para fechar o Popup
        public Popup? PopupInstance { get; set; }

        [ObservableProperty] private string title = "Novo Veículo";
        [ObservableProperty] private string vehicleId;
        [ObservableProperty] private string name;
        [ObservableProperty] private string brand;
        [ObservableProperty] private string model;
        [ObservableProperty] private string year;
        [ObservableProperty] private string plate;
        [ObservableProperty] private string currentOdometer;
        [ObservableProperty] private bool isEditMode = false;

        public VehicleAddViewModel(VehicleService vehicleService, ApiService apiService)
        {
            _vehicleService = vehicleService;
            _apiService = apiService;
        }

        partial void OnCurrentOdometerChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digitsOnly)) return;

            if (decimal.TryParse(digitsOnly, out decimal parsed))
            {
                string formatted = parsed.ToString("N0", new System.Globalization.CultureInfo("pt-BR"));
                if (CurrentOdometer != formatted)
                {
                    CurrentOdometer = formatted;
                }
            }
        }

        partial void OnPlateChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var cleaned = new string(value.ToUpper().Where(char.IsLetterOrDigit).ToArray());
            if (cleaned.Length > 7) cleaned = cleaned.Substring(0, 7);
            if (cleaned.Length == 0) return;

            string formattedPlate = cleaned;

            if (cleaned.Length == 7 && Regex.IsMatch(cleaned, "^[A-Z]{3}[0-9]{4}$"))
            {
                formattedPlate = $"{cleaned.Substring(0, 3)}-{cleaned.Substring(3, 4)}";
            }
            else if (cleaned.Length == 7 && Regex.IsMatch(cleaned, "^[A-Z]{3}[0-9]{1}[A-Z]{1}[0-9]{2}$"))
            {
                formattedPlate = cleaned;
            }

            if (Plate != formattedPlate)
            {
                Plate = formattedPlate;
            }
        }

        partial void OnVehicleIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Title = "Editar Veículo";
                IsEditMode = true;
                Task.Run(() => LoadVehicleData(value));
            }
        }

        private async Task LoadVehicleData(string id)
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var json = await _apiService.GetAsync<string>($"vehicles/{id}");
                if (!string.IsNullOrEmpty(json))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var vehicle = JsonSerializer.Deserialize<Vehicle>(json, options);

                    if (vehicle != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Name = vehicle.Name;
                            Brand = vehicle.Brand;
                            Model = vehicle.Model;
                            Year = vehicle.Year == 0 ? DateTime.Now.Year.ToString() : vehicle.Year.ToString();
                            Plate = vehicle.Plate;
                            CurrentOdometer = vehicle.CurrentOdometer.ToString("N0", new System.Globalization.CultureInfo("pt-BR"));
                        });
                    }
                }
            });
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Model) || string.IsNullOrWhiteSpace(CurrentOdometer) || string.IsNullOrWhiteSpace(Plate))
            {
                await App.Current.MainPage.DisplayAlert("Aviso", "Preencha Nome, Modelo, Placa e KM Atual.", "OK");
                return;
            }

            int yearValue = DateTime.Now.Year;
            if (!string.IsNullOrWhiteSpace(Year))
            {
                if (!int.TryParse(Year, out yearValue) || yearValue < 1900 || yearValue > DateTime.Now.Year + 1)
                {
                    await App.Current.MainPage.DisplayAlert("Erro", $"Ano inválido (mínimo 1900).", "OK");
                    return;
                }
            }

            if (!decimal.TryParse(CurrentOdometer, System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("pt-BR"), out decimal odoValue))
            {
                odoValue = 0;
            }

            string unformattedPlate = new string(Plate.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            if (unformattedPlate.Length != 7 || (!Regex.IsMatch(unformattedPlate, "^[A-Z]{3}[0-9]{4}$") && !Regex.IsMatch(unformattedPlate, "^[A-Z]{3}[0-9]{1}[A-Z]{1}[0-9]{2}$")))
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Formato de placa inválido (necessário ABC-1234 ou ABC1D23).", "OK");
                return;
            }

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var dto = new Vehicle
                {
                    Id = string.IsNullOrEmpty(VehicleId) ? Guid.Empty : Guid.Parse(VehicleId),
                    Name = Name.Trim(),
                    Brand = Brand?.Trim(),
                    Model = Model.Trim(),
                    Year = yearValue,
                    Plate = unformattedPlate,
                    CurrentOdometer = odoValue
                };

                bool success;
                if (string.IsNullOrEmpty(VehicleId))
                {
                    success = await _vehicleService.SaveVehicleAsync(dto);
                }
                else
                {
                    success = await _vehicleService.UpdateVehicleAsync(VehicleId, dto);
                }

                if (success)
                {
                    WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
                    PopupInstance?.Close(); // Fecha o Popup!
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Falha ao salvar veículo.", "OK");
                }
            });
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert("Excluir Veículo", $"Deseja apagar o veículo '{Name}'?", "Sim", "Não");
            if (!confirm) return;

            var success = await _vehicleService.DeleteVehicleAsync(VehicleId);
            if (success)
            {
                WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
                PopupInstance?.Close(); // Fecha o Popup!
            }
        }

        [RelayCommand]
        private void GoBack() => PopupInstance?.Close(); // Fecha o Popup!
    }
}
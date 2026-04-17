using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Maui.Views;

namespace controle_ja_mobile.ViewModels
{
    public partial class CreditCardAddViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public Popup? PopupInstance { get; set; }

        [ObservableProperty] private string title = "Novo Cartão";
        [ObservableProperty] private string cardId;
        [ObservableProperty] private string name;
        [ObservableProperty] private string limit;
        [ObservableProperty] private string closeDay;
        [ObservableProperty] private string bestDay;

        [ObservableProperty] private bool isEditMode = false;

        public ObservableCollection<string> AvailableColors { get; } = new(UIConstants.AvailableColors);

        // Cravado direto na raiz, sem tela de seleção.
        private readonly string _selectedIcon = "credit_card";

        [ObservableProperty] private string selectedColor;
        [ObservableProperty] private bool isColorSelectorOpen;

        public CreditCardAddViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SelectedColor = AvailableColors.FirstOrDefault(c => c == "#9C27B0") ?? AvailableColors.First();
        }

        partial void OnLimitChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digitsOnly)) return;

            if (decimal.TryParse(digitsOnly, out decimal parsed))
            {
                parsed /= 100;
                string formatted = parsed.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));

                if (Limit != formatted)
                {
                    Limit = formatted;
                }
            }
        }

        partial void OnCardIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Title = "Editar Cartão";
                IsEditMode = true;
                Task.Run(() => LoadCardData(value));
            }
        }

        private async Task LoadCardData(string id)
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var json = await _apiService.GetAsync<string>($"cards/{id}");
                if (!string.IsNullOrEmpty(json))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var card = JsonSerializer.Deserialize<CreditCard>(json, options);

                    if (card != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Name = card.Name;
                            Limit = card.TotalLimit.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
                            CloseDay = card.CloseDay.ToString();
                            BestDay = card.BestDay.ToString();
                            SelectedColor = card.Color ?? "#9C27B0";
                        });
                    }
                }
            });
        }

        [RelayCommand] private void OpenColorSelector() => IsColorSelectorOpen = true;
        [RelayCommand] private void CloseColorSelector() => IsColorSelectorOpen = false;
        [RelayCommand] private void SelectColor(string color) { SelectedColor = color; IsColorSelectorOpen = false; }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Limit) ||
                string.IsNullOrWhiteSpace(CloseDay) || string.IsNullOrWhiteSpace(BestDay))
            {
                await App.Current.MainPage.DisplayAlert("Aviso", "Preencha todos os campos.", "OK");
                return;
            }

            if (!decimal.TryParse(Limit, System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("pt-BR"), out decimal limitValue) || limitValue <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Limite inválido.", "OK");
                return;
            }

            if (!int.TryParse(CloseDay, out int cDay) || cDay < 1 || cDay > 31 ||
                !int.TryParse(BestDay, out int bDay) || bDay < 1 || bDay > 31)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Dias devem ser entre 1 e 31.", "OK");
                return;
            }

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var dto = new
                {
                    name = Name.Trim(),
                    limit = limitValue,
                    closeDay = cDay,
                    bestDay = bDay,
                    icon = _selectedIcon,
                    color = SelectedColor
                };

                string result;
                if (string.IsNullOrEmpty(CardId))
                    result = await _apiService.PostAsync<object>("cards", dto);
                else
                    result = await _apiService.PutAsync<object>($"cards/{CardId}", dto);

                if (!string.IsNullOrEmpty(result))
                {
                    WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
                    PopupInstance?.Close();
                }
            });
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert("Excluir Cartão", $"Deseja apagar o cartão '{Name}'?", "Sim", "Não");
            if (!confirm) return;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var result = await _apiService.DeleteAsync($"cards/{CardId}");
                if (!string.IsNullOrEmpty(result))
                {
                    WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
                    PopupInstance?.Close();
                }
            });
        }

        [RelayCommand]
        private void GoBack() => PopupInstance?.Close();
    }
}
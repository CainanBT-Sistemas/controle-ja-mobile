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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.ViewModels
{
    public partial class CreditCardsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public Popup? PopupInstance { get; set; }

        public ObservableCollection<CreditCard> Cards { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        public CreditCardsViewModel(ApiService apiService)
        {
            _apiService = apiService;

            // MÁGICA: Escuta quando alguém salva/deleta um cartão e recarrega a lista sozinho!
            WeakReferenceMessenger.Default.Register<GlobalRefreshMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _ = LoadCardsAsync();
                });
            });
        }

        [RelayCommand]
        public async Task LoadCardsAsync()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var response = await _apiService.GetAsync<string>("cards");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Cards.Clear();
                    if (!string.IsNullOrEmpty(response))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var list = JsonSerializer.Deserialize<List<CreditCard>>(response, options);

                        if (list != null)
                        {
                            foreach (var card in list)
                            {
                                GenerateCardChart(card);
                                Cards.Add(card);
                            }
                        }
                    }
                });
                IsRefreshing = false;
            });
        }

        private void GenerateCardChart(CreditCard card)
        {
            if (card.UsedAmount > 0)
            {
                card.HasChartData = true;
                var colors = new[] { SKColor.Parse("#00E676"), SKColor.Parse("#2979FF"), SKColor.Parse("#FFAB00"), SKColor.Parse("#FF5252") };

                var entries = new List<ChartEntry>
                {
                    new ChartEntry((float)(card.UsedAmount * 0.4m)) { Label = "Mercado", ValueLabel = "40%", Color = colors[0], ValueLabelColor = colors[0] },
                    new ChartEntry((float)(card.UsedAmount * 0.3m)) { Label = "Lazer", ValueLabel = "30%", Color = colors[1], ValueLabelColor = colors[1] },
                    new ChartEntry((float)(card.UsedAmount * 0.2m)) { Label = "Transp.", ValueLabel = "20%", Color = colors[2], ValueLabelColor = colors[2] },
                    new ChartEntry((float)(card.UsedAmount * 0.1m)) { Label = "Outros", ValueLabel = "10%", Color = colors[3], ValueLabelColor = colors[3] },
                };

                card.CategoryChart = new DonutChart
                {
                    Entries = entries,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 20,
                    HoleRadius = 0.60f,
                    LabelColor = SKColor.Parse("#94A3B8")
                };
            }
            else
            {
                card.HasChartData = false;
            }
        }

        private void ShowCardPopup(string? cardId = null)
        {
            var vm = IPlatformApplication.Current?.Services.GetService<CreditCardAddViewModel>();
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(cardId)) vm.CardId = cardId;

                var popup = new Views.Popups.CreditCardAddPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToAddCard()
        {
            ShowCardPopup();
        }

        [RelayCommand]
        public void OpenCardDetails(CreditCard card)
        {
            if (card != null) ShowCardPopup(card.Id.ToString());
        }

        [RelayCommand]
        public void GoBack() => PopupInstance?.Close();
    }
}
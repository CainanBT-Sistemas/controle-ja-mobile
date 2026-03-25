using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
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
        public ObservableCollection<CreditCard> Cards { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        public CreditCardsViewModel(ApiService apiService)
        {
            _apiService = apiService;
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
                                GenerateCardChart(card); // Gerando o gráfico novamente
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

        [RelayCommand]
        public async Task GoToAddCard()
        {
            await Shell.Current.GoToAsync(nameof(CreditCardAddPage));
        }

        [RelayCommand]
        public async Task OpenCardDetails(CreditCard card)
        {
            await Shell.Current.GoToAsync($"{nameof(CreditCardAddPage)}?id={card.Id}");
        }

        [RelayCommand]
        public async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}
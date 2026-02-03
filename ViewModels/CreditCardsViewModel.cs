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
    public partial class CreditCardsViewModel : BaseViewModel
    {
        private readonly CreditCardService _creditCardService;
        public ObservableCollection<CreditCard> Cards { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        // Campos do Formulário
        [ObservableProperty] private string newCardName;
        [ObservableProperty] private string newCardLimit;
        [ObservableProperty] private string newCardCloseDay;
        [ObservableProperty] private string newCardBestDay;

        public CreditCardsViewModel(CreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        [RelayCommand]
        public async Task LoadCardsAsync()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var cards = await _creditCardService.GetCreditCardsAsync();
                Cards.Clear();
                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        GenerateCardChart(card); // Gera o gráfico
                        Cards.Add(card);
                    }
                }
                IsRefreshing = false;
            }, showLoading: false);
        }

        private void GenerateCardChart(CreditCard card)
        {
            // Se o cartão tem uso, geramos um gráfico fictício para ilustrar
            if (card.UsedAmount > 0)
            {
                card.HasChartData = true;

                // Cores do tema
                var colors = new[] { SKColor.Parse("#00E676"), SKColor.Parse("#2979FF"), SKColor.Parse("#FFAB00"), SKColor.Parse("#FF5252") };

                // Dados Simulados (distribui o valor usado em categorias fictícias por enquanto)
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
                    LabelColor = SKColor.Parse("#94A3B8") // Cinza claro
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
        public async Task SaveCardAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCardName) || string.IsNullOrWhiteSpace(NewCardLimit) ||
                string.IsNullOrWhiteSpace(NewCardCloseDay) || string.IsNullOrWhiteSpace(NewCardBestDay))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha todos os campos do cartão.", "OK");
                return;
            }

            if (!decimal.TryParse(NewCardLimit, out decimal limit))
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Limite inválido. Digite apenas números.", "OK");
                return;
            }

            if (!int.TryParse(NewCardCloseDay, out int closeDay) || closeDay < 1 || closeDay > 31)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Dia de fechamento inválido (1-31).", "OK");
                return;
            }

            if (!int.TryParse(NewCardBestDay, out int bestDay) || bestDay < 1 || bestDay > 31)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Dia de vencimento inválido (1-31).", "OK");
                return;
            }

            var newCard = new CreditCard
            {
                Name = NewCardName,
                TotalLimit = limit,
                CloseDay = closeDay,
                BestDay = bestDay
            };

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var success = await _creditCardService.SaveCreditCardAsync(newCard);
                if (success)
                {
                    await App.Current.MainPage.DisplayAlert("Sucesso", "Cartão adicionado!", "OK");

                    NewCardName = "";
                    NewCardLimit = "";
                    NewCardCloseDay = "";
                    NewCardBestDay = "";

                    await Shell.Current.GoToAsync("..");
                    await LoadCardsAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Ops", "Não foi possível criar o cartão.", "OK");
                }
            });
        }
    }
}
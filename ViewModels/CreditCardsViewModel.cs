using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class CreditCardsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<CreditCard> Cards { get; } = new();

        [ObservableProperty] private bool isRefreshing;

        // Campos do Formulário
        [ObservableProperty] private string newCardName;
        [ObservableProperty] private string newCardLimit;
        [ObservableProperty] private string newCardCloseDay;
        [ObservableProperty] private string newCardBestDay;

        public CreditCardsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadCards()
        {
            // Passamos 'false' no showLoading se estivermos usando o RefreshView, 
            // senão o loading global aparece em cima do spinner nativo.
            await ExecuteAsync(async () =>
            {
                try
                {
                    var list = await _apiService.GetAsync<List<CreditCard>>("cards");
                    Cards.Clear();
                    if (list != null)
                    {
                        foreach (var card in list) Cards.Add(card);
                    }
                }
                finally
                {
                    IsRefreshing = false; // Garante que o spinner para
                }
            }, showLoading: false);
        }

        [RelayCommand]
        public async Task GoToAddCard()
        {
            await Shell.Current.GoToAsync(nameof(CreditCardAddPage));
        }

        [RelayCommand]
        public async Task SaveCard()
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

            await ExecuteAsync(async () =>
            {
                var newCardDto = new
                {
                    name = NewCardName,
                    limit = limit,
                    closeDay = closeDay,
                    bestDay = bestDay
                };

                var result = await _apiService.PostAsync<CreditCard>("cards", newCardDto);

                if (result != null)
                {
                    await App.Current.MainPage.DisplayAlert("Sucesso", "Cartão adicionado!", "OK");

                    // Limpa campos
                    NewCardName = "";
                    NewCardLimit = "";
                    NewCardCloseDay = "";
                    NewCardBestDay = "";

                    await Shell.Current.GoToAsync("..");
                    await LoadCards();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Ops", "Não foi possível criar o cartão.", "OK");
                }
            });
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.Views.Publics;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class CreditCardsViewModel : BaseViewModel
    {
        private readonly CreditCardService _creditCardService;
        public ObservableCollection<CreditCard> Cards { get; } = new();

        [ObservableProperty] private bool isRefreshing;
        
        // Menu superior (configurações)
        [ObservableProperty] private bool isSettingsMenuVisible;
        [ObservableProperty] private string userName;

        // Campos do Formulário
        [ObservableProperty] private string newCardName;
        [ObservableProperty] private string newCardLimit;
        [ObservableProperty] private string newCardCloseDay;
        [ObservableProperty] private string newCardBestDay;

        public CreditCardsViewModel(CreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
            UserName = Preferences.Get("UserName", "Usuário");
        }

        // COMANDOS DO MENU SUPERIOR
        [RelayCommand]
        public void ToggleSettingsMenu()
        {
            IsSettingsMenuVisible = !IsSettingsMenuVisible;
        }

        [RelayCommand]
        public void CloseSettingsMenu()
        {
            if (IsSettingsMenuVisible)
            {
                IsSettingsMenuVisible = false;
            }
        }

        [RelayCommand]
        public async Task PerformLogout()
        {
            IsSettingsMenuVisible = false;
            
            bool confirm = await Shell.Current.DisplayAlert("Sair", "Tem certeza que deseja desconectar da sua conta?", "Sim", "Não");
            if (!confirm) return;

            Preferences.Remove("AuthToken");
            Preferences.Remove("UserName");

            var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }

        [RelayCommand]
        public async Task GoToProfile()
        {
            IsSettingsMenuVisible = false;
            await Shell.Current.DisplayAlert("Meu Perfil", "A edição de perfil estará disponível em breve.", "OK");
        }

        [RelayCommand]
        public async Task LoadCardsAsync()
        {
            // Passamos 'false' no showLoading se estivermos usando o RefreshView, 
            // senão o loading global aparece em cima do spinner nativo.
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var cards = await _creditCardService.GetCreditCardsAsync();
                Cards.Clear();
                if (cards != null)
                {
                    foreach (var card in cards) Cards.Add(card);
                }
                IsRefreshing = false;
            }, showLoading: false);
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

                    // Limpa campos
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
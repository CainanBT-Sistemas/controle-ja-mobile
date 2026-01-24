using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.ViewModels
{
    public partial class CreditCardsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        // Lista para a tela principal
        public ObservableCollection<CreditCard> Cards { get; } = new();

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isRefreshing;

        // --- PROPRIEDADES PARA O FORMULÁRIO DE CADASTRO ---
        [ObservableProperty] private string newCardName;
        [ObservableProperty] private string newCardLimit;
        [ObservableProperty] private string newCardCloseDay; // Dia Fechamento
        [ObservableProperty] private string newCardBestDay;  // Dia Vencimento

        public CreditCardsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadCards()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                // GET /cards
                var list = await _apiService.GetAsync<List<CreditCard>>("cards");

                Cards.Clear();
                if (list != null)
                {
                    foreach (var card in list) Cards.Add(card);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro Cards: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        // Navega para a tela de adicionar (precisa criar a View CreditCardAddPage)
        [RelayCommand]
        public async Task GoToAddCard()
        {
            // Registre essa rota no AppShell.xaml.cs depois!
            await Shell.Current.GoToAsync(nameof(CreditCardAddPage));
        }

        // --- O COMMAND QUE VOCÊ PEDIU (SALVAR) ---
        [RelayCommand]
        public async Task SaveCard()
        {
            // 1. Validações Básicas
            if (string.IsNullOrWhiteSpace(NewCardName) ||
                string.IsNullOrWhiteSpace(NewCardLimit) ||
                string.IsNullOrWhiteSpace(NewCardCloseDay) ||
                string.IsNullOrWhiteSpace(NewCardBestDay))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha todos os campos do cartão.", "OK");
                return;
            }

            // 2. Conversão de Tipos
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

            IsLoading = true;

            try
            {
                // 3. Monta o Objeto (Igual ao CreditCardDTO do Java)
                var newCardDto = new
                {
                    name = NewCardName,
                    limit = limit,
                    closeDay = closeDay,
                    bestDay = bestDay
                };

                // 4. Envia para a API (POST /cards)
                // Endpoint confirmado no arquivo CreditCardController.java
                var result = await _apiService.PostAsync<CreditCard>("cards", newCardDto);

                if (result != null)
                {
                    await App.Current.MainPage.DisplayAlert("Sucesso", "Cartão adicionado!", "OK");

                    // Limpa o formulário
                    NewCardName = "";
                    NewCardLimit = "";
                    NewCardCloseDay = "";
                    NewCardBestDay = "";

                    // Volta para a lista
                    await Shell.Current.GoToAsync("..");

                    // Atualiza a lista
                    await LoadCards();
                }
                else
                {
                    // Se der erro (ex: limite de 2 cartões do plano Free), o Backend manda 400
                    // O ApiService captura e loga, mas aqui mostramos um alerta genérico
                    await App.Current.MainPage.DisplayAlert("Ops", "Não foi possível criar o cartão. Verifique se atingiu o limite ou se os dados estão corretos.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro", $"Falha de comunicação: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}

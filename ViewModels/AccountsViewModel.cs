using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace controle_ja_mobile.ViewModels
{
    public partial class AccountsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Account> Accounts { get; } = new();

        [ObservableProperty] private bool isRefreshing;
        [ObservableProperty] private bool isEmptyStateVisible;

        public AccountsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadAccounts()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                List<Account> accountsList = null;
                var result = await _apiService.GetAsync<string>("accounts");
                if(!string.IsNullOrWhiteSpace(result))
                {
                    accountsList = JsonSerializer.Deserialize<List<Account>>(result);
                }
                Accounts.Clear();
                if (accountsList != null && accountsList.Any())
                {
                    IsEmptyStateVisible = false;
                    foreach (var acc in accountsList) Accounts.Add(acc);
                }
                else
                {
                    IsEmptyStateVisible = true;
                }
                
                IsRefreshing = false;
            });
        }

        [RelayCommand]
        public async Task GoToAddAccount()
        {
             await Shell.Current.DisplayAlert("Adicionar", "Em breve", "OK");
        }

        [RelayCommand]
        public async Task EditAccount(Account account)
        {
             await Shell.Current.DisplayAlert("Editar", $"Editar {account.Name}", "OK");
        }

        [RelayCommand]
        public async Task DeleteAccount(Account account)
        {
             bool confirm = await Shell.Current.DisplayAlert("Excluir", $"Apagar {account.Name}?", "Sim", "Não");
             if(!confirm) return;

             await ExecuteWithErrorHandlingAsync(async () => {
                 var result = await _apiService.DeleteAsync($"accounts/{account.Id}");
                 if (!string.IsNullOrWhiteSpace(result))
                 {
                     if (result.Contains("Registro excluído com sucesso."))
                     {
                         await LoadAccounts();
                     }
                 }
             });
        }

        [RelayCommand]
        public async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}

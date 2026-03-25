using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                var result = await _apiService.GetAsync<string>("accounts");
                List<Account> accountsList = null;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    accountsList = JsonSerializer.Deserialize<List<Account>>(result, options);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
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
                });

                IsRefreshing = false;
            });
        }

        [RelayCommand]
        public async Task GoToAddAccount()
        {
            await Shell.Current.GoToAsync(nameof(AccountAddPage));
        }

        [RelayCommand]
        public async Task OpenAccountDetails(Account account)
        {
            // Abre a tela de Edição/Detalhes
            await Shell.Current.GoToAsync($"{nameof(AccountAddPage)}?id={account.Id}");
        }

        [RelayCommand]
        public async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}
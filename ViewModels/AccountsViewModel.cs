using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.ViewModels
{
    public partial class AccountsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public Popup? PopupInstance { get; set; }

        public ObservableCollection<Account> Accounts { get; } = new();

        [ObservableProperty] private bool isRefreshing;
        [ObservableProperty] private bool isEmptyStateVisible;

        public AccountsViewModel(ApiService apiService)
        {
            _apiService = apiService;

            // MÁGICA: Escuta quando alguém salva/deleta uma conta e recarrega a lista sozinho!
            WeakReferenceMessenger.Default.Register<GlobalRefreshMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _ = LoadAccounts();
                });
            });
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

        private void ShowAccountPopup(string? accountId = null)
        {
            var vm = IPlatformApplication.Current?.Services.GetService<AccountAddViewModel>();
            if (vm != null)
            {
                if (!string.IsNullOrEmpty(accountId))
                {
                    vm.AccountId = accountId;
                }

                var popup = new Views.Popups.AccountAddPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToAddAccount()
        {
            ShowAccountPopup();
        }

        [RelayCommand]
        public void OpenAccountDetails(Account account)
        {
            if (account != null)
            {
                ShowAccountPopup(account.Id.ToString());
            }
        }

        [RelayCommand]
        public void GoBack() => PopupInstance?.Close();
    }
}
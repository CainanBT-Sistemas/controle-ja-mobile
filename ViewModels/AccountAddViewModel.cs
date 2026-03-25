using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace controle_ja_mobile.ViewModels
{
    [QueryProperty(nameof(AccountId), "id")]
    public partial class AccountAddViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private string title = "Nova Conta";
        [ObservableProperty] private string accountId;
        [ObservableProperty] private string name;
        [ObservableProperty] private string institution;

        // 1. Alterado para string para aceitar a máscara formatada (R$ 0,00)
        [ObservableProperty] private string balance;

        [ObservableProperty] private bool isDefault;

        [ObservableProperty] private bool isEditMode = false;

        public bool CanDelete => IsEditMode && !IsDefault;

        // 2. Removido o "Cartão de Crédito"
        public List<string> AccountTypes { get; } = new() { "Carteira", "Banco", "Poupança" };
        [ObservableProperty] private string selectedAccountType = "Banco";

        public ObservableCollection<string> AvailableIcons { get; } = new(UIConstants.AvailableIcons);
        public ObservableCollection<string> AvailableColors { get; } = new(UIConstants.AvailableColors);

        [ObservableProperty] private string selectedIcon;
        [ObservableProperty] private string selectedColor;
        [ObservableProperty] private bool isColorSelectorOpen;
        [ObservableProperty] private bool isIconSelectorOpen;

        public AccountAddViewModel(ApiService apiService)
        {
            _apiService = apiService;
            SelectedIcon = "account_balance";
            SelectedColor = AvailableColors.FirstOrDefault(c => c == "#42A5F5") ?? AvailableColors.First();
        }

        // 3. Gatilho da máscara de dinheiro em tempo real
        partial void OnBalanceChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Extrai apenas os números do que o usuário digitou
            var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digitsOnly)) return;

            // Divide por 100 para criar os centavos e aplica a formatação brasileira
            if (decimal.TryParse(digitsOnly, out decimal parsed))
            {
                parsed /= 100;
                string formatted = parsed.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));

                // Previne o loop infinito
                if (Balance != formatted)
                {
                    Balance = formatted;
                }
            }
        }

        partial void OnAccountIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Title = "Editar Conta";
                IsEditMode = true;
                _ = LoadAccountData(value);
            }
        }

        private async Task LoadAccountData(string id)
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var json = await _apiService.GetAsync<string>($"accounts/{id}");
                if (!string.IsNullOrEmpty(json))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var account = JsonSerializer.Deserialize<Account>(json, options);

                    if (account != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Name = account.Name;

                            // 4. Carrega o saldo formatado. (Use account.CurrentBalance se o modelo Account usar esse nome em vez de Balance)
                            Balance = account.Balance.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));

                            Institution = account.Institution;
                            SelectedIcon = account.Icon ?? "account_balance";
                            SelectedColor = account.Color ?? "#42A5F5";
                            IsDefault = account.IsDefault;

                            SelectedAccountType = account.Type switch
                            {
                                AccountType.WALLET => "Carteira",
                                AccountType.BANK => "Banco",
                                AccountType.SAVINGS => "Poupança",
                                _ => "Banco"
                            };

                            OnPropertyChanged(nameof(CanDelete));
                        });
                    }
                }
            });
        }

        [RelayCommand] private void OpenColorSelector() => IsColorSelectorOpen = true;
        [RelayCommand] private void CloseColorSelector() => IsColorSelectorOpen = false;
        [RelayCommand] private void OpenIconSelector() => IsIconSelectorOpen = true;
        [RelayCommand] private void CloseIconSelector() => IsIconSelectorOpen = false;
        [RelayCommand] private void SelectIcon(string icon) { SelectedIcon = icon; IsIconSelectorOpen = false; }
        [RelayCommand] private void SelectColor(string color) { SelectedColor = color; IsColorSelectorOpen = false; }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Aviso", "Por favor, informe o nome da conta.", "OK");
                return;
            }

            // 5. Converte o texto com vírgula de volta para decimal antes de enviar para a API
            if (!decimal.TryParse(Balance, System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("pt-BR"), out decimal balanceValue))
            {
                balanceValue = 0; // Previne crash se o campo estiver vazio
            }

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                string enumType = SelectedAccountType switch
                {
                    "Carteira" => "WALLET",
                    "Poupança" => "SAVINGS",
                    _ => "BANK"
                };

                var dto = new
                {
                    name = Name.Trim(),
                    type = enumType,
                    institution = Institution?.Trim(),
                    initialBalance = balanceValue, // Usando o decimal convertido
                    icon = SelectedIcon,
                    color = SelectedColor,
                    isDefault = IsDefault
                };

                string result;
                if (string.IsNullOrEmpty(AccountId))
                    result = await _apiService.PostAsync<object>("accounts", dto);
                else
                    result = await _apiService.PutAsync<object>($"accounts/{AccountId}", dto);

                if (!string.IsNullOrEmpty(result))
                    await Shell.Current.GoToAsync("..");
            });
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool confirm = await Shell.Current.DisplayAlert("Excluir Conta", $"Deseja apagar a conta '{Name}' e as transações ligadas a ela?", "Sim", "Não");
            if (!confirm) return;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var result = await _apiService.DeleteAsync($"accounts/{AccountId}");
                if (!string.IsNullOrEmpty(result))
                {
                    await Shell.Current.GoToAsync("..");
                }
            });
        }

        [RelayCommand]
        private async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}
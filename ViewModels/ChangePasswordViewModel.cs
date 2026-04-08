using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;

namespace controle_ja_mobile.ViewModels
{
    public partial class ChangePasswordViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty] private string currentPassword;
        [ObservableProperty] private string newPassword;
        [ObservableProperty] private string confirmPassword;

        public ChangePasswordViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task SavePassword()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Shell.Current.DisplayAlert("Aviso", "Preencha todos os campos.", "OK");
                return;
            }

            if (NewPassword.Length < 6)
            {
                await Shell.Current.DisplayAlert("Aviso", "A nova senha deve ter no mínimo 6 caracteres.", "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Aviso", "As novas senhas não conferem.", "OK");
                return;
            }

            if (CurrentPassword == NewPassword)
            {
                await Shell.Current.DisplayAlert("Aviso", "A nova senha não pode ser igual à atual.", "OK");
                return;
            }

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var success = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Senha alterada com segurança!", "OK");

                    // Limpa os campos por segurança
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;

                    // Volta para a tela de Perfil
                    await Shell.Current.GoToAsync("..");
                }
            });
        }

        [RelayCommand]
        private async Task GoBack() => await Shell.Current.GoToAsync("..");
    }
}
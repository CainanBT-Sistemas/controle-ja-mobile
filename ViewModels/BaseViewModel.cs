using CommunityToolkit.Mvvm.ComponentModel;
using controle_ja_mobile.Helpers;
using System;
using System.Threading.Tasks;

namespace controle_ja_mobile.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isBusy;

        // Método centralizado para executar operações com tratamento de erro amigável
        protected async Task ExecuteAsync(Func<Task> operation, bool showLoading = true)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                if (showLoading) IsLoading = true;

                await operation();
            }
            catch (Exception ex)
            {
                var error = ErrorHandler.Parse(ex);
                await Application.Current.MainPage.DisplayAlert(error.Title, error.Message, "OK");
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }
    }
}


using CommunityToolkit.Mvvm.ComponentModel;
using controle_ja_mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace controle_ja_mobile.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isBusy;

        protected async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, bool showLoading = true)
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

        protected async Task NavigateToAsync(string route, IDictionary<string, object> parameters = null)
        {
            try
            {
                IsLoading = true;

                if (parameters != null)
                {
                    await Shell.Current.GoToAsync(route, parameters);
                }
                else
                {
                    await Shell.Current.GoToAsync(route);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro de Navegação", $"Falha ao abrir a tela: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
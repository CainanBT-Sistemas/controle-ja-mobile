using controle_ja_mobile.Configs;
using controle_ja_mobile.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace controle_ja_mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService()
        {
            _baseUrl = AppConstants.BaseUrl;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // Padronizei o PostAsync para Task<string> para bater com seu ViewModel
        public async Task<string> PostAsync<T>(string endpoint, object data)
        {
            await AddAuthenticationHeaderAsync(endpoint);
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            if (!response.IsSuccessStatusCode)
            {
                await HandlerErrors(response);
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAsync<T>(string endpoint)
        {
            await AddAuthenticationHeaderAsync(endpoint);
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                await HandlerErrors(response);
            }
            return await response.Content.ReadAsStringAsync();
        }

        // CORRIGIDO: Adicionado Autenticação e HandlerErrors para ficar igual aos outros
        public async Task<string> PutAsync<T>(string endpoint, object data)
        {
            await AddAuthenticationHeaderAsync(endpoint);
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            if (!response.IsSuccessStatusCode)
            {
                await HandlerErrors(response);
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteAsync(string endpoint)
        {
            await AddAuthenticationHeaderAsync(endpoint);
            var response = await _httpClient.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                await HandlerErrors(response);
            }
            return await response.Content.ReadAsStringAsync();
        }

        // Mudado para Task para podermos dar 'await' nas chamadas
        private async Task HandlerErrors(HttpResponseMessage response)
        {
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<UserFriendlyError>();
                if (errorResponse != null)
                {
                    string errors = "";
                    foreach (var e in errorResponse.Message.Split(", "))
                    {
                        if (!string.IsNullOrEmpty(e))
                        {
                            errors += string.IsNullOrEmpty(errors) ? e : "\n" + e;
                        }
                    }
                    await App.Current.MainPage.DisplayAlert(errorResponse.Title, errors, "OK");
                }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Ocorreu um erro inesperado.", "OK");
            }
        }

        private async Task AddAuthenticationHeaderAsync(string endpoint)
        {
            // Ajustado para ignorar rotas de login/registro
            if (endpoint.Contains("/auth") || endpoint.Contains("/users/register"))
            {
                return;
            }

            var token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
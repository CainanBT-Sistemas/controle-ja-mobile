using controle_ja_mobile.Configs;
using controle_ja_mobile.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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

        public async Task<string> PostAsync<T>(string endpoint, object data)
        {
            AddAuthenticationHeaderAsync(endpoint);
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            if (!response.IsSuccessStatusCode)
            {
                HandlerErrors(response);
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<String> GetAsync<T>(string endpoint)
        {
            AddAuthenticationHeaderAsync(endpoint);
            var response =  await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                HandlerErrors(response);
            }
            return await response.Content.ReadAsStringAsync();
        }

        private async void HandlerErrors(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<UserFriendlyError>();
            if (errorResponse != null) {
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

        private async Task AddAuthenticationHeaderAsync(string endpoint)
        {
            if(endpoint.Contains("/auth") || endpoint.Contains("/users/register"))
            {
                return;
            }

            var token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

    }
}
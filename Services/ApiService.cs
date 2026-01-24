using controle_ja_mobile.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;

        public ApiService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(AppConstants.BaseUrl)
            };
            _serializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private void AddAuthorizationHeader(string endpoint)
        {
            if (endpoint.Contains("/auth") || endpoint.Contains("/register"))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return;
            }

            var token = Preferences.Get(AppConstants.AuthStorageKey, string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader(endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro GET {endpoint}: {ex.Message}");
            }
            return default;
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader(endpoint);

                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro POST {endpoint}: {ex.Message}");
            }
            return default;
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader(endpoint);
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro PUT {endpoint}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                AddAuthorizationHeader(endpoint);
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro DELETE {endpoint}: {ex.Message}");
                return false;
            }
        }
    }
}

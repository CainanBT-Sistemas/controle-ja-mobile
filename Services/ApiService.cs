using controle_ja_mobile.Configs;
using controle_ja_mobile.Helpers;
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
                // CORREÇÃO: Define limite de 10 segundos. 
                // Sem isso, o app fica rodando para sempre se o servidor cair.
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            // Não usamos try/catch aqui para deixar o erro subir para a ViewModel
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return default;

                return await response.Content.ReadFromJsonAsync<T>();
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                try
                {
                    var apiError = JsonSerializer.Deserialize<ApiErrorResponse>(errorJson);
                    throw new Exception(apiError?.Message ?? "Erro desconhecido.");
                }
                catch
                {
                    throw new Exception("Erro desconhecido: " + errorJson);
                }
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                try
                {
                    var apiError = JsonSerializer.Deserialize<ApiErrorResponse>(errorJson);
                    throw new Exception(apiError?.Message ?? "Erro desconhecido.");
                }
                catch
                {
                    throw new Exception("Erro desconhecido: " + errorJson);
                }
            }
        }
    }
}
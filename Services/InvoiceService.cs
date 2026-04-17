using controle_ja_mobile.Models;
using System.Text.Json;

namespace controle_ja_mobile.Services
{
    public class InvoiceService
    {
        private readonly ApiService _apiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public InvoiceService(ApiService apiService)
        {
            _apiService = apiService;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<InvoiceDetailsDTO> GetInvoiceDetailsAsync(Guid cardId, int month, int year)
        {
            try
            {
                var response = await _apiService.GetAsync<string>($"invoices/card/{cardId}/month/{month}/year/{year}");
                if (!string.IsNullOrWhiteSpace(response) && !response.Contains("Not Found") && !response.Contains("404"))
                {
                    return JsonSerializer.Deserialize<InvoiceDetailsDTO>(response, _jsonOptions);
                }
                return null;
            }
            catch { return null; }
        }

        // === NOVA ROTA DE ANTECIPAÇÃO ===
        public async Task<List<AdvanceablePurchaseDTO>> GetAdvanceablePurchasesAsync(Guid cardId, int month, int year)
        {
            try
            {
                var response = await _apiService.GetAsync<string>($"invoices/card/{cardId}/month/{month}/year/{year}/advanceable");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    return JsonSerializer.Deserialize<List<AdvanceablePurchaseDTO>>(response, _jsonOptions) ?? new();
                }
                return new List<AdvanceablePurchaseDTO>();
            }
            catch { return new List<AdvanceablePurchaseDTO>(); }
        }

        public async Task<bool> ProcessRefundAsync(Guid invoiceId, RefundRequestDTO dto)
        {
            try
            {
                var response = await _apiService.PostAsync<string>($"invoices/{invoiceId}/refund", dto);
                return response != null && !response.Contains("Erro");
            }
            catch { return false; }
        }

        public async Task<bool> AdvanceInstallmentsAsync(Guid invoiceId, AdvanceRequestDTO dto)
        {
            try
            {
                var response = await _apiService.PostAsync<string>($"invoices/{invoiceId}/advance", dto);
                return response != null && !response.Contains("Erro");
            }
            catch { return false; }
        }
    }
}
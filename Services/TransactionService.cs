using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class TransactionService
    {
        private readonly ApiService _apiService;

        public TransactionService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Transaction>?> GetTransactionsAsync(long start, long end)
        {
            try
            {
                string endpoint = $"transactions?start={start}&end={end}";
                var response = await _apiService.GetAsync<string>(endpoint);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var transactionsResponse = JsonSerializer.Deserialize<List<Transaction>>(response);
                    if (transactionsResponse != null)
                    {
                        return transactionsResponse;
                    }
                }
                return new List<Transaction>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new List<Transaction>();
        }

        public async Task<bool> SaveTransactionAsync(Transaction transaction)
        {
            try
            {
                var response = await _apiService.PostAsync<string>("transactions", transaction);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var transactionResponse = JsonSerializer.Deserialize<Transaction>(response);
                    if (transactionResponse != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async void ErrrorHandler(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            var errorResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(errorResponse);
        }
    }
}

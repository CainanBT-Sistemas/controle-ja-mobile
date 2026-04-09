using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class TransactionService
    {
        private readonly ApiService _apiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public TransactionService(ApiService apiService)
        {
            _apiService = apiService;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<List<Transaction>?> GetTransactionsAsync(long? start = null, long? end = null)
        {
            try
            {
                string url = "transactions";

                if (start.HasValue && end.HasValue)
                {
                    url += $"?start={start}&end={end}";
                }

                var response = await _apiService.GetAsync<string>(url);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    var transactionsResponse = JsonSerializer.Deserialize<List<Transaction>>(response, _jsonOptions);
                    if (transactionsResponse != null) return transactionsResponse;
                }
                return new List<Transaction>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== ERRO AO LER JSON: " + ex.Message + " ====");
                return new List<Transaction>();
            }
        }

        public async Task<bool> SaveTransactionAsync(Transaction transaction)
        {
            try
            {
                var response = await _apiService.PostAsync<string>("transactions", transaction);
                return !string.IsNullOrWhiteSpace(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== ERRO AO SALVAR: " + ex.Message + " ====");
                return false;
            }
        }

        // NOVO: Aceita o parâmetro do Efeito Cascata
        public async Task<bool> UpdateTransactionAsync(Guid id, Transaction transaction, bool updateFuture = false)
        {
            try
            {
                var response = await _apiService.PutAsync<string>($"transactions/{id}?updateFuture={updateFuture}", transaction);
                return !string.IsNullOrWhiteSpace(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== ERRO AO ATUALIZAR: " + ex.Message + " ====");
                return false;
            }
        }

        // NOVO: Método de Deleção com Cancelamento de Cascata
        public async Task<bool> DeleteTransactionAsync(Guid id, bool cancelFuture = false)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"transactions/{id}?cancelFuture={cancelFuture}");
                return !string.IsNullOrWhiteSpace(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== ERRO AO DELETAR: " + ex.Message + " ====");
                return false;
            }
        }
    }
}
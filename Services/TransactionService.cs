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
        private readonly JsonSerializerOptions _jsonOptions; // <-- Tradutor criado

        public TransactionService(ApiService apiService)
        {
            _apiService = apiService;

            // Aqui nós ensinamos o C# a ler "RECEITA" e a ignorar letras maiúsculas/minúsculas no JSON
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

                // Se o ViewModel mandar as datas, adiciona na URL
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
                // Usamos as opções aqui também para garantir que ele envie "RECEITA" como texto para o Java
                var response = await _apiService.PostAsync<string>("transactions", transaction);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== ERRO AO SALVAR: " + ex.Message + " ====");
                return false;
            }
        }

        public async Task<bool> UpdateTransactionAsync(Guid id, Transaction transaction)
        {
            try
            {
                var response = await _apiService.PutAsync<string>($"transactions/{id}", transaction);
                if (!string.IsNullOrWhiteSpace(response)) return true;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== ERRO AO ATUALIZAR: " + ex.Message + " ====");
                return false;
            }
        }
    }
}
using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class CreditCardService
    {
        private readonly ApiService _apiService;

        public CreditCardService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<CreditCard>> GetCreditCardsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>("cards");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CreditCard>>() ?? new List<CreditCard>();
                }

                return new List<CreditCard>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<CreditCard>();
            }
        }

        public async Task<bool> SaveCreditCardAsync(CreditCard card)
        {
            try
            {
                var response = await _apiService.PostAsync<HttpResponseMessage>("cards", card);
                //return response.IsSuccessStatusCode;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}

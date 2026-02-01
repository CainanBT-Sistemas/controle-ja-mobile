using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class AccountService
    {
        private readonly ApiService _apiService;

        public AccountService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>("accounts");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Account>>() ?? new List<Account>();
                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Account>();
            }
        }

        public async Task<bool> SaveAccountAsync(Account account)
        {
            try
            {
                //var response = await _apiService.PostAsync<HttpResponseMessage>("accounts", account);
                //if (response.IsSuccessStatusCode)
                //{
                //    return true;
                //}
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

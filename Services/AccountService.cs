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
                var response = await _apiService.GetAsync<string>("accounts");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var accountResponse = JsonSerializer.Deserialize<List<Account>>(response);
                    if(accountResponse != null)
                    {
                        return accountResponse;
                    }
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
                var response = await _apiService.PostAsync<string>("accounts", account);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var accountResponse =  JsonSerializer.Deserialize<Account>(response);
                    if(accountResponse != null && accountResponse?.Id != null)
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

    }
}

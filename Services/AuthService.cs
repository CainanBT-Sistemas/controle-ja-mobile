using controle_ja_mobile.Configs;
using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class AuthService
    {
        private readonly ApiService _apiService;

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> loginAsync(string email, string password)
        {
            var loginData = new LoginRequest { email = email, password = password };
            var result = await _apiService.PostAsync<UserResponse>("auth", loginData);
            if (result?.Tokens?.AccessToken != null)
            {
                Preferences.Set(AppConstants.AuthStorageKey, result.Tokens.AccessToken);
                string nameToSave = !string.IsNullOrEmpty(result.Username) ? result.Username : "Usuário";
                Preferences.Set("UserName", nameToSave);
                return true;
            }

            return false;
        }
        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            var registerData = new
            {
                username = name,
                email = email,
                password = password,
                role = UserRole.USER
            };
            var result = await _apiService.PostAsync<UserResponse>("users/register", registerData);
            return result != null;
        }
    }
}

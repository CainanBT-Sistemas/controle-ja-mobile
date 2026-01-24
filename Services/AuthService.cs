using controle_ja_mobile.Configs;
using controle_ja_mobile.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography; // Necessário para a segurança do Google

namespace controle_ja_mobile.Services
{
    public class AuthService
    {
        private readonly ApiService _apiService;

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        // --- LOGIN COM GOOGLE (FLUXO SEGURO PKCE) ---
        public async Task<bool> LoginWithGoogleAsync()
        {
            try
            {
                string clientId = "336338674705-v2hntbb10rnof4mrrkf8k03c2ot1opv9.apps.googleusercontent.com";
                string redirectUri = "com.googleusercontent.apps.336338674705-v2hntbb10rnof4mrrkf8k03c2ot1opv9:/";

                string codeVerifier = GenerateCodeVerifier();
                string codeChallenge = GenerateCodeChallenge(codeVerifier);

                string authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                                 $"?client_id={clientId}" +
                                 "&response_type=code" +
                                 $"&redirect_uri={redirectUri}" +
                                 $"&code_challenge={codeChallenge}" +
                                 "&code_challenge_method=S256" +
                                 "&scope=email%20profile%20openid";

                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(authUrl),
                    new Uri(redirectUri));

                if (result?.Properties != null && result.Properties.ContainsKey("code"))
                {
                    string code = result.Properties["code"];
                    string accessToken = await ExchangeCodeForToken(code, codeVerifier, clientId, redirectUri);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        return await FinalizeGoogleLogin(accessToken);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro Google Auth: {ex.Message}");
            }
            return false;
        }

        private async Task<bool> FinalizeGoogleLogin(string accessToken)
        {
            var userInfo = await GetGoogleUserInfo(accessToken);
            if (userInfo != null)
            {
                var payload = new
                {
                    email = userInfo.email,
                    googleId = userInfo.id,
                    displayName = userInfo.name,
                    photoUrl = userInfo.picture
                };

                var apiResult = await _apiService.PostAsync<UserResponse>("auth/google", payload);

                if (apiResult?.Tokens?.AccessToken != null)
                {
                    Preferences.Set(AppConstants.AuthStorageKey, apiResult.Tokens.AccessToken);
                    Preferences.Set("UserName", apiResult.Username);
                    return true;
                }
            }
            return false;
        }

        // Troca o código pelo token (Obrigatório no fluxo PKCE)
        private async Task<string> ExchangeCodeForToken(string code, string codeVerifier, string clientId, string redirectUri)
        {
            try
            {
                var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code_verifier", codeVerifier)
                });

                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
                    return tokenData?.access_token;
                }
            }
            catch { }
            return null;
        }

        // --- MÉTODOS DE SUPORTE (LOGIN NORMAL E REGISTRO) ---
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
            var registerData = new { username = name, email = email, password = password, role = UserRole.USER };
            var result = await _apiService.PostAsync<UserResponse>("users/register", registerData);
            return result != null;
        }

        private async Task<GoogleUser> GetGoogleUserInfo(string accessToken)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var json = await client.GetStringAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                return JsonSerializer.Deserialize<GoogleUser>(json);
            }
            catch { return null; }
        }

        // CRIPTOGRAFIA PARA O GOOGLE
        private string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(bytes); }
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private class GoogleTokenResponse { public string access_token { get; set; } }
        private class GoogleUser { public string id { get; set; } public string email { get; set; } public string name { get; set; } public string picture { get; set; } }
    }
}
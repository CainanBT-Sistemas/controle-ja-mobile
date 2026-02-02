using controle_ja_mobile.Configs;
using controle_ja_mobile.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace controle_ja_mobile.Services
{
    public class AuthService
    {
        private readonly ApiService _apiService;

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Login Normal (E-mail e Senha)
        public async Task<bool> loginAsync(string email, string password)
        {
            try
            {
                var loginData = new { email, password };
                var result = await _apiService.PostAsync<string>("auth/login", loginData);
                if (!string.IsNullOrEmpty(result))
                {
                    var userResponse = JsonSerializer.Deserialize<UserResponse>(result);
                    if (userResponse != null && userResponse?.Id != null)
                    {
                        await SaveAuthTokenAsync(userResponse.Tokens.AccessToken, userResponse.Tokens.RefreshToken, userResponse.Username);
                        return true;
                    }
                }
                return false;

            }
            catch (TaskCanceledException ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. Tente novamente mais tarde.", "OK");
               
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Erro Google Auth: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> loginWithTokenAsync(string token)
        {
            try
            {
                var loginData = new { token };
                var result = await _apiService.PostAsync<string>("auth/auto-login", loginData);
                if (!string.IsNullOrEmpty(result))
                {
                    var userResponse = JsonSerializer.Deserialize<UserResponse>(result);
                    if (userResponse != null && !string.IsNullOrEmpty(userResponse.Tokens?.AccessToken))
                    {
                        await SaveAuthTokenAsync(userResponse.Tokens.AccessToken, userResponse.Tokens.RefreshToken, userResponse.Username);
                        return true;
                    }
                }
                return false;

            }
            catch (TaskCanceledException ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. Tente novamente mais tarde.", "OK");

            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Token inválido"))
                {
                    SaveAuthTokenAsync(null,null, null);
                    await App.Current.MainPage.DisplayAlert("Acesso Negado", "Token Inválido, acesse novamente.", "OK");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"Erro Google Auth: {ex.Message}");
            }
            return false;
        }

        // Registro
        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registerData = new { username, email, password };
                var result = await _apiService.PostAsync<HttpResponseMessage>("users/register", registerData);
                if (!string.IsNullOrWhiteSpace(result)){
                    var userResponse = JsonSerializer.Deserialize<UserResponse>(result);
                    if(userResponse?.Id != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                if (ex.Message.Equals("Cannot access a closed Stream."))
                {
                    return false;
                }

                Console.WriteLine(ex);
            }
            return false;
            
        }

        // LOGIN COM GOOGLE USANDO O FLUXO PKCE (SEGURO)
        public async Task<bool> LoginWithGoogleAsync()
        {
            try
            {
                string clientId = AppSecrets.GoogleClientId;
                string redirectUri = $"{AppSecrets.GoogleRedirectScheme}:/";

                string codeVerifier = GenerateCodeVerifier();
                string codeChallenge = GenerateCodeChallenge(codeVerifier);

                string authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                                 $"?client_id={clientId}" +
                                 "&response_type=code" +
                                 $"&redirect_uri={redirectUri}" +
                                 $"&code_challenge={codeChallenge}" +
                                 "&code_challenge_method=S256" +
                                 "&scope=openid%20email%20profile";

                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(authUrl),
                    new Uri(redirectUri));

                if (result?.Properties != null && result.Properties.ContainsKey("code"))
                {
                    string code = result.Properties["code"];
                    string accessToken = await ExchangeCodeForToken(code, codeVerifier, clientId, redirectUri);

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        var userInfoResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
                        if (userInfoResponse.IsSuccessStatusCode)
                        {
                            var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
                            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoJson);

                            var googlePayload = new
                            {
                                email = userInfo.email,
                                googleId = userInfo.sub,
                                displayName = userInfo.name,
                                photoUrl = userInfo.picture
                            };

                            var apiResult = await _apiService.PostAsync<string>("auth/google", googlePayload);
                            if (!string.IsNullOrWhiteSpace(apiResult))
                            {
                                var userResponse = JsonSerializer.Deserialize<UserResponse>(apiResult);
                                if (userResponse != null && !string.IsNullOrEmpty(userResponse.Tokens?.AccessToken))
                                {
                                    await SaveAuthTokenAsync(userResponse.Tokens.AccessToken, userResponse.Tokens.RefreshToken, userResponse.Username);
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            catch (TaskCanceledException ex)
            {
                if (ex.StackTrace?.Contains("WebAuthenticator") == true)
                {
                    await App.Current.MainPage.DisplayAlert("Cancelado", "Login cancelado pelo usuário.", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. Tente novamente mais tarde.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Token inválido"))
                {
                    SaveAuthTokenAsync(null,null, null);
                    await App.Current.MainPage.DisplayAlert("Acesso Negado", "Token Inválido, acesse novamente.", "OK");
                    return false;
                }

                await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. " + ex.Message, "OK");
            }

            return false;
        }

        private class GoogleUserInfo
        {
            public string sub { get; set; } // Google ID
            public string email { get; set; }
            public string name { get; set; }
            public string picture { get; set; }
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

        // Métodos auxiliares PKCE
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

        // Classe auxiliar para desserializar o token do Google
        private class GoogleTokenResponse
        {
            public string access_token { get; set; }
        }

        public async Task SaveAuthTokenAsync(string token, string refreshToken, string username)
        {
            if (token == null)
                SecureStorage.Remove("auth_token");
            else
                await SecureStorage.SetAsync("auth_token", token);
            if (refreshToken == null)
                SecureStorage.Remove("refresh_token");
            else
                await SecureStorage.SetAsync("refresh_token", refreshToken);
            if (username == null)
                Preferences.Remove("UserName");
            else
                Preferences.Set("UserName", username);

        }

        private async Task<bool> remover(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            var errorResponse = await response.Content.ReadAsStringAsync();

            if (errorResponse.Contains("A senha deve ter no mínimo 6 caracteres"))
            {
                await App.Current.MainPage.DisplayAlert("Falha ao fazer login", "A senha deve ter no mínimo 6 caracteres", "ok");
                return false;
            }

            if (errorResponse.Contains("O nome de usuário é obrigatório"))
            {
                await App.Current.MainPage.DisplayAlert("Falha ao fazer login", "O nome de usuário é obrigatório", "ok");
                return false;

            }
            if (errorResponse.Contains("Formato de email inválido"))
            {
                await App.Current.MainPage.DisplayAlert("Falha ao fazer login", "Formato de email inválido", "ok");
                return false;

            }
            

            return false;
        }
    }    
}

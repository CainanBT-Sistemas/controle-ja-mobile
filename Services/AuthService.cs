using controle_ja_mobile.Configs;
using controle_ja_mobile.Models;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using controle_ja_mobile.Views.Privates;
using Microsoft.Maui.Controls;

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
                var result = await _apiService.PostAsync<UserResponse>("auth/login", loginData);
                if (result != null && !string.IsNullOrEmpty(result.Tokens?.AccessToken))
                {
                    Preferences.Set("AuthToken", result.Tokens.AccessToken);
                    Preferences.Set("UserName", result.Username);
                    return true;
                }

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

        // Registro
        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            var registerData = new { name, email, password };
            var result = await _apiService.PostAsync<UserResponse>("auth/register", registerData);
            return result != null;
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

                            var apiResult = await _apiService.PostAsync<UserResponse>("auth/google", googlePayload);

                            if (apiResult != null && !string.IsNullOrEmpty(apiResult.Tokens?.AccessToken))
                            {
                                Preferences.Set("AuthToken", apiResult.Tokens.AccessToken);
                                Preferences.Set("UserName", apiResult.Username);
                                return true;
                            }
                        }
                    }
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

        public async Task<bool> AuthenticateWithBiometricsAsync()
        {
            var result = await CrossFingerprint.Current.AuthenticateAsync(
            new AuthenticationRequestConfiguration("Autenticação Biométrica", "Use FaceID ou impressão digital para continuar")
        );

            if (result.Authenticated)
            {
                await App.Current.MainPage.DisplayAlert("Sucesso", "Autenticação realizada com sucesso", "OK");
                return true;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Erro", "A autenticação biométrica falhou.", "OK");
                return false;
            }
        }
    }    
}

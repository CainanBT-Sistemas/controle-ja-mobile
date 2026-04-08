using controle_ja_mobile.Configs;
using controle_ja_mobile.Models;
using System.Net.Http.Headers;
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

        // --- LOGIN & AUTO-LOGIN ---

        public async Task<bool> loginAsync(string email, string password)
        {
            try
            {
                var loginData = new { email, password };
                var result = await _apiService.PostAsync<string>("auth", loginData);
                if (!string.IsNullOrEmpty(result))
                {
                    var userResponse = JsonSerializer.Deserialize<UserResponse>(result);
                    if (userResponse != null && userResponse?.Id != null)
                    {
                        // IMPORTANTE: Adicionei o userResponse.Id aqui para ser salvo
                        await SaveAuthTokenAsync(userResponse.Tokens.AccessToken, userResponse.Tokens.RefreshToken, userResponse.Username, userResponse.Email, userResponse.Id);
                        return true;
                    }
                }
                return false;
            }
            catch (TaskCanceledException)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. Tente novamente mais tarde.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro Login: {ex.Message}");
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
                        await SaveAuthTokenAsync(userResponse.Tokens.AccessToken, userResponse.Tokens.RefreshToken, userResponse.Username, userResponse.Email, userResponse.Id);
                        return true;
                    }
                }
                return false;
            }
            catch (TaskCanceledException)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. Tente novamente mais tarde.", "OK");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Token inválido"))
                {
                    await SaveAuthTokenAsync(null, null, null);
                    await App.Current.MainPage.DisplayAlert("Acesso Negado", "Sessão expirada, acesse novamente.", "OK");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"Erro Auto-Login: {ex.Message}");
            }
            return false;
        }

        // --- REGISTRO ---

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registerData = new { username, email, password };
                var result = await _apiService.PostAsync<string>("users/register", registerData);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (result.Contains("A senha deve ter no mínimo") ||
                        result.Contains("O nome de usuário é obrigatório") ||
                        result.Contains("Formato de email inválido") ||
                        result.Contains("já está em uso"))
                    {
                        await HandleBackendErrors(result);
                        return false;
                    }

                    try
                    {
                        var userResponse = JsonSerializer.Deserialize<UserResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (userResponse?.Id != null)
                        {
                            return true;
                        }
                    }
                    catch (JsonException)
                    {
                        await HandleBackendErrors(result);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                if (!ex.Message.Equals("Cannot access a closed Stream."))
                    Console.WriteLine(ex);
            }
            return false;
        }

        // --- GESTÃO DE PERFIL, SENHA E EXCLUSÃO ---

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                var payload = new { currentPassword, newPassword };
                var result = await _apiService.PutAsync<string>("users/change-password", payload);
                return !string.IsNullOrEmpty(result) && result.Contains("sucesso", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("incorreta") || ex.Message.Contains("inválida"))
                    await App.Current.MainPage.DisplayAlert("Erro", "A senha atual informada está incorreta.", "OK");
                else
                    await App.Current.MainPage.DisplayAlert("Erro", "Não foi possível alterar a senha.", "OK");
                return false;
            }
        }

        public async Task<bool> UpdateProfileAsync(string newUsername)
        {
            try
            {
                var payload = new { username = newUsername};
                var result = await _apiService.PutAsync<string>("users/profile", payload);

                if (!string.IsNullOrEmpty(result))
                {
                    Preferences.Set("UserName", newUsername);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Falha ao atualizar o perfil. Este e-mail já pode estar em uso.", "OK");
                return false;
            }
        }

        // NOVO: MÉTODO PARA DELETAR CONTA (SOFT DELETE)
        public async Task<bool> DeleteAccountAsync()
        {
            try
            {
                // Busca o ID do usuário salvo nas preferências
                var userId = Preferences.Get("UserId","");

                if (string.IsNullOrEmpty(userId))
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Não foi possível identificar o usuário logado.", "OK");
                    return false;
                }

                // Chama a rota DELETE: controle_ja_api/v1/users/{id}
                await _apiService.DeleteAsync<string>($"users/{userId}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao deletar conta: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> resetDataUser()
        {
            try
            {
                var userId = Preferences.Get("UserId", "");

                if (string.IsNullOrEmpty(userId))
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Não foi possível identificar o usuário logado.", "OK");
                    return false;
                }
                var result = await _apiService.GetAsync<string>($"users/reset/{userId}");
                return !string.IsNullOrEmpty(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao deletar conta: {ex.Message}");
                return false;
            }
        }


        // --- GOOGLE OAUTH PKCE ---

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
                                    await SaveAuthTokenAsync(userResponse.Tokens.AccessToken, userResponse.Tokens.RefreshToken, userResponse.Username, userResponse.Email, userResponse.Id);
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
                    await App.Current.MainPage.DisplayAlert("Cancelado", "Login cancelado pelo usuário.", "OK");
                else
                    await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. Tente novamente mais tarde.", "OK");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Token inválido"))
                {
                    await SaveAuthTokenAsync(null, null, null);
                    await App.Current.MainPage.DisplayAlert("Acesso Negado", "Sessão expirada, acesse novamente.", "OK");
                    return false;
                }
                await App.Current.MainPage.DisplayAlert("Erro", "Servidor indisponível. " + ex.Message, "OK");
            }
            return false;
        }

        private class GoogleUserInfo
        {
            public string sub { get; set; }
            public string email { get; set; }
            public string name { get; set; }
            public string picture { get; set; }
        }

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

        private class GoogleTokenResponse
        {
            public string access_token { get; set; }
        }

        public async Task SaveAuthTokenAsync(string token, string refreshToken, string username, string email = null, string userId = null)
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

            if (email == null)
                Preferences.Remove("UserEmail");
            else
                Preferences.Set("UserEmail", email);

            if (userId == null)
                Preferences.Remove("UserId");
            else
                Preferences.Set("UserId", userId);
        }

        private async Task HandleBackendErrors(string errorResponse)
        {
            if (errorResponse.Contains("A senha deve ter no mínimo 6 caracteres"))
                await App.Current.MainPage.DisplayAlert("Falha", "A senha deve ter no mínimo 6 caracteres", "OK");
            else if (errorResponse.Contains("O nome de usuário é obrigatório"))
                await App.Current.MainPage.DisplayAlert("Falha", "O nome de usuário é obrigatório", "OK");
            else if (errorResponse.Contains("Formato de email inválido"))
                await App.Current.MainPage.DisplayAlert("Falha", "Formato de email inválido", "OK");
            else if (errorResponse.Contains("já está em uso"))
                await App.Current.MainPage.DisplayAlert("Falha", "Este email já está em uso.", "OK");
            else
                await App.Current.MainPage.DisplayAlert("Erro", "Ocorreu um erro ao processar sua solicitação.", "OK");
        }
    }
}
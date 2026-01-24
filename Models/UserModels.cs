using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string password { get; set; } = string.Empty;
    }

    // Para receber a resposta do Login
    public class UserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("tokens")]
        public AuthTokens? Tokens { get; set; }
    }

    public class AuthTokens
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;

namespace controle_ja_mobile.Helpers
{
    public class UserFriendlyError
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public static class ErrorHandler
    {
        public static UserFriendlyError Parse(Exception ex)
        {
            // 1. Sem Internet (Verificação local)
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return new UserFriendlyError
                {
                    Title = "Sem Conexão",
                    Message = "Parece que você está offline.\n\nVerifique seu Wi-Fi ou dados móveis e tente novamente."
                };
            }

            // 2. Erros de Requisição HTTP (Servidor respondeu com erro)
            if (ex is HttpRequestException httpEx)
            {
                // Tenta pegar o Status Code (404, 500, 401)
                if (httpEx.StatusCode.HasValue)
                {
                    switch (httpEx.StatusCode.Value)
                    {
                        case HttpStatusCode.Unauthorized: // 401
                            return new UserFriendlyError
                            {
                                Title = "Acesso Negado",
                                Message = "Sua sessão expirou ou suas credenciais são inválidas.\n\nPor favor, faça login novamente."
                            };
                        case HttpStatusCode.InternalServerError: // 500
                        case HttpStatusCode.ServiceUnavailable:  // 503
                            return new UserFriendlyError
                            {
                                Title = "Problema no Servidor",
                                Message = "Nossos servidores estão passando por instabilidades no momento.\n\nPor favor, aguarde alguns minutos e tente novamente."
                            };
                        case HttpStatusCode.NotFound: // 404
                            return new UserFriendlyError
                            {
                                Title = "Não Encontrado",
                                Message = "O recurso que você tentou acessar não existe ou foi movido."
                            };
                    }
                }

                // Se não tem status code, geralmente é servidor fora do ar ou DNS
                return new UserFriendlyError
                {
                    Title = "Servidor Indisponível",
                    Message = "Não conseguimos conectar ao servidor do Controle Já.\n\nVerifique se não há bloqueios na sua rede ou tente mais tarde."
                };
            }

            // 3. Timeout (Demorou demais)
            if (ex is TaskCanceledException)
            {
                return new UserFriendlyError
                {
                    Title = "Tempo Esgotado",
                    Message = "O servidor demorou muito para responder.\n\nSua conexão pode estar lenta. Tente novamente."
                };
            }

            // 4. Erro Genérico (O "Throw" que não mapeamos)
            return new UserFriendlyError
            {
                Title = "Ops! Algo deu errado",
                Message = $"Ocorreu um erro inesperado.\nDetalhe técnico: {ex.Message}\n\nTente reiniciar o aplicativo."
            };
        }
    }
}

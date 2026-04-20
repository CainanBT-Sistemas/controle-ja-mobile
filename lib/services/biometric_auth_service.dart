/// Serviço de autenticação biométrica.
///
/// Migrado de BiometricAuthService.cs.
/// Requer plugin `local_auth` para funcionar em produção.
/// Este é um placeholder estrutural — a implementação real será
/// adicionada quando o plugin nativo for integrado.
class BiometricAuthService {
  /// Verifica se autenticação biométrica está disponível no dispositivo.
  Future<bool> isBiometricAvailable() async {
    // TODO: Implementar com local_auth quando o plugin for adicionado
    return false;
  }

  /// Autentica o usuário usando biometria (digital/face) ou PIN do dispositivo.
  Future<bool> authenticate({
    String reason = 'Autentique-se para continuar',
  }) async {
    // TODO: Implementar com local_auth quando o plugin for adicionado
    return false;
  }
}

using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace controle_ja_mobile.Services
{
    public class BiometricAuthService
    {
        private readonly IFingerprint _fingerprint;

        public BiometricAuthService()
        {
            _fingerprint = CrossFingerprint.Current;
        }

        /// <summary>
        /// Checks if biometric authentication is available on the device
        /// </summary>
        public async Task<bool> IsBiometricAvailableAsync()
        {
            return await _fingerprint.IsAvailableAsync();
        }

        /// <summary>
        /// Authenticates user using biometric (fingerprint/face recognition) or device PIN as fallback
        /// </summary>
        /// <param name="reason">Message to display to the user</param>
        /// <returns>True if authentication successful, false otherwise</returns>
        public async Task<bool> AuthenticateAsync(string reason = "Autentique-se para continuar")
        {
            try
            {
                // Check if biometric is available
                var available = await _fingerprint.IsAvailableAsync();
                
                if (!available)
                {
                    // Biometric not available on device
                    return false;
                }

                // Configure authentication request
                var request = new AuthenticationRequestConfiguration(reason, reason)
                {
                    // Allow fallback to device credentials (PIN/Pattern/Password)
                    AllowAlternativeAuthentication = true,
                    
                    // Cancel button text
                    CancelTitle = "Cancelar",
                    
                    // Fallback button text (for devices that show this option)
                    FallbackTitle = "Usar PIN",
                    
                    // Confirmation required (for some biometric types)
                    ConfirmationRequired = false
                };

                // Perform authentication
                var result = await _fingerprint.AuthenticateAsync(request);

                return result.Authenticated;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Biometric authentication error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the type of biometric authentication available
        /// </summary>
        public async Task<FingerprintAuthenticationType> GetAuthenticationTypeAsync()
        {
            return await _fingerprint.GetAuthenticationTypeAsync();
        }
    }
}

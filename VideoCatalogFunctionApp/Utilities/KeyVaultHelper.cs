using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace VideoCatalogFunctionApp.Utilities
{
    /// <summary>
    /// Provides methods to interact with Azure Key Vault for retrieving secrets.
    /// </summary>
    public class KeyVaultHelper
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultHelper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultHelper"/> class.
        /// </summary>
        /// <param name="secretClient">The <see cref="SecretClient"/> used to interact with Azure Key Vault.</param>
        /// <param name="logger">The <see cref="ILogger"/> for logging operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="secretClient"/> or <paramref name="logger"/> is null.</exception>
        public KeyVaultHelper(SecretClient secretClient, ILogger<KeyVaultHelper> logger)
        {
            _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously retrieves a secret from Azure Key Vault.
        /// </summary>
        /// <param name="secretName">The name of the secret to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the value of the secret.</returns>
        /// <exception cref="Exception">Thrown if there is an error retrieving the secret.</exception>
        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                // Log the attempt to retrieve the secret
                _logger.LogInformation($"Attempting to retrieve secret: {secretName}");

                // Retrieve the secret from Key Vault
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);

                // Log successful retrieval of the secret
                _logger.LogInformation($"Successfully retrieved secret: {secretName}");

                // Return the value of the secret
                return secret.Value;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during retrieval
                _logger.LogError(ex, $"Error retrieving secret: {secretName}");

                // Re-throw the exception to be handled by the caller
                throw;
            }
        }
    }
}

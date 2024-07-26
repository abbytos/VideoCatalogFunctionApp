using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VideoCatalogFunctionApp.Utilities
{
    /// <summary>
    /// Provides configuration-related functionalities for accessing secrets from Key Vault
    /// and retrieving configuration values used in the application.
    /// </summary>
    public class ConfigurationHelper
    {
        private readonly KeyVaultHelper _keyVaultHelper;
        private readonly ILogger<ConfigurationHelper> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationHelper"/> class.
        /// </summary>
        /// <param name="keyVaultHelper">The helper class for retrieving secrets from Key Vault.</param>
        /// <param name="logger">Logger for logging errors and information.</param>
        /// <param name="configuration">Configuration for accessing application settings.</param>
        public ConfigurationHelper(KeyVaultHelper keyVaultHelper, ILogger<ConfigurationHelper> logger, IConfiguration configuration)
        {
            _keyVaultHelper = keyVaultHelper;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the blob container name and storage connection string from Key Vault.
        /// </summary>
        /// <returns>
        /// A tuple containing the container name and connection string if successfully retrieved,
        /// or null if either value is missing or an error occurs.
        /// </returns>
        public async Task<(string ContainerName, string ConnectionString)?> GetConfigurationAsync()
        {
            try
            {
                // Asynchronously retrieve container name and connection string from Key Vault
                var containerNameSecretName = _configuration["BlobStorage:ContainerName"];
                var connectionStringSecretName = _configuration["BlobStorage:ConnectionStringSecretName"];

                if (string.IsNullOrEmpty(containerNameSecretName) || string.IsNullOrEmpty(connectionStringSecretName))
                {
                    _logger.LogError("Configuration is incomplete: ContainerName or ConnectionStringSecretName is missing.");
                    return null;
                }

                var containerName = await _keyVaultHelper.GetSecretAsync(containerNameSecretName);
                var connectionString = await _keyVaultHelper.GetSecretAsync(connectionStringSecretName);

                if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Configuration is incomplete: ContainerName or ConnectionString is missing.");
                    return null;
                }

                return (containerName, connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving configuration: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the maximum allowed file size for uploads.
        /// </summary>
        /// <returns>
        /// The maximum file size in bytes.
        /// </returns>
        public long GetMaxFileSize()
        {
            // Retrieve and convert max file size from configuration (in MB) to bytes
            var maxFileSizeMB = int.Parse(_configuration["FileSettings:MaxFileSizeMB"]);
            return maxFileSizeMB * 1024 * 1024;
        }
    }
}

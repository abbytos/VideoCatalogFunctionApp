using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using VideoCatalogFunctionApp.Utilities;

namespace VideoCatalogFunctionApp.ListFilesFunction
{
    /// <summary>
    /// Handles listing of video files stored in Azure Blob Storage.
    /// </summary>
    public class ListFilesFunction
    {
        private readonly ConfigurationHelper _configurationHelper;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<ListFilesFunction> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFilesFunction"/> class.
        /// </summary>
        /// <param name="configurationHelper">The configuration helper for retrieving configuration values.</param>
        /// <param name="blobServiceClient">The BlobServiceClient for accessing Blob Storage.</param>
        /// <param name="logger">The logger for logging messages.</param>
        public ListFilesFunction(ConfigurationHelper configurationHelper, BlobServiceClient blobServiceClient, ILogger<ListFilesFunction> logger)
        {
            _configurationHelper = configurationHelper;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// Handles HTTP GET requests to list video files in Azure Blob Storage.
        /// </summary>
        /// <param name="req">The HTTP request data.</param>
        /// <returns>An <see cref="HttpResponseData"/> containing the list of video files or an error message.</returns>
        [Function("ListFilesFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ListVideos")] HttpRequestData req)
        {
            _logger.LogInformation("Listing files.");

            try
            {
                // Retrieve configuration values
                var config = await _configurationHelper.GetConfigurationAsync();
                if (!config.HasValue)
                {
                    return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Configuration is not properly set up.");
                }

                var (containerName, connectionString) = config.Value;

                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Check if the blob container exists
                if (!await blobContainerClient.ExistsAsync())
                {
                    _logger.LogWarning("Blob container '{ContainerName}' does not exist.", containerName);
                    return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.NotFound, "Blob container does not exist.");
                }

                // List all blobs in the container
                var files = new List<VideoFileModel>();
                await foreach (var blob in blobContainerClient.GetBlobsAsync())
                {
                    files.Add(new VideoFileModel
                    {
                        FileName = blob.Name,
                        FileSize = blob.Properties.ContentLength ?? 0
                    });
                }

                // Return the list of video files as JSON
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(files);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while listing files: {Message}", ex.Message);
                return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, $"An error occurred while processing your request: {ex.Message}");
            }
        }
    }
}

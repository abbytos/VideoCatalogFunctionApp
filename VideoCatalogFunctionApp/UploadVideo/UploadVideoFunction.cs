using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using VideoCatalogFunctionApp.Utilities;

namespace VideoCatalogFunctionApp.Functions
{
    /// <summary>
    /// Handles the upload of video files to Azure Blob Storage.
    /// </summary>
    public class UploadVideoFunction
    {
        private readonly ConfigurationHelper _configurationHelper;
        private readonly ILogger<UploadVideoFunction> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadVideoFunction"/> class.
        /// </summary>
        /// <param name="configurationHelper">The configuration helper for retrieving configuration values.</param>
        /// <param name="logger">The logger for logging messages.</param>
        public UploadVideoFunction(ConfigurationHelper configurationHelper, ILogger<UploadVideoFunction> logger)
        {
            _configurationHelper = configurationHelper;
            _logger = logger;
        }

        /// <summary>
        /// Handles HTTP POST requests to upload video files.
        /// </summary>
        /// <param name="req">The HTTP request data.</param>
        /// <returns>An <see cref="HttpResponseData"/> indicating the result of the upload operation.</returns>
        [Function("UploadVideo")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UploadVideo")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request.");

            // Retrieve configuration values
            var config = await _configurationHelper.GetConfigurationAsync();
            if (!config.HasValue)
            {
                return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Configuration is not properly set up.");
            }

            var (containerName, connectionString) = config.Value;
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Check if the blob container exists
            if (!await blobContainerClient.ExistsAsync())
            {
                _logger.LogError("Blob container '{ContainerName}' does not exist.", containerName);
                return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.NotFound, "Blob container does not exist.");
            }

            // Validate Content-Type header
            var contentType = req.Headers.GetValues("Content-Type")?.FirstOrDefault();
            if (string.IsNullOrEmpty(contentType) || !contentType.Contains("multipart/form-data"))
            {
                return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid form content. Expecting 'multipart/form-data'.");
            }

            // Parse boundary from Content-Type header
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            var boundary = mediaTypeHeaderValue.Parameters
                             .FirstOrDefault(p => p.Name.Equals("boundary", System.StringComparison.OrdinalIgnoreCase))
                             ?.Value;

            if (string.IsNullOrEmpty(boundary))
            {
                return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, "Missing boundary in Content-Type header.");
            }

            var reader = new MultipartReader(boundary, req.Body);
            MultipartSection section;

            // Process each section of the multipart content
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                // Check file size
                if (section.Body.Length > _configurationHelper.GetMaxFileSize())
                {
                    return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, "File size exceeds the 200MB limit.");
                }

                // Validate and upload the file
                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                {
                    if (contentDisposition.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName))
                    {
                        var fileName = contentDisposition.FileName.Trim('"');
                        if (!fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                        {
                            return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid file. Only .mp4 files are allowed.");
                        }

                        var blobClient = blobContainerClient.GetBlobClient(fileName);
                        await blobClient.UploadAsync(section.Body, overwrite: true);

                        var successResponse = req.CreateResponse(HttpStatusCode.OK);
                        await successResponse.WriteStringAsync("File uploaded successfully.");
                        return successResponse;
                    }
                }
            }

            return HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, "No valid file data in request.");
        }
    }
}

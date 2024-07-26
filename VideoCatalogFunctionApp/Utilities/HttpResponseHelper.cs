using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace VideoCatalogFunctionApp.Utilities
{
    /// <summary>
    /// Provides helper methods for creating HTTP responses in Azure Functions.
    /// </summary>
    public static class HttpResponseHelper
    {
        /// <summary>
        /// Creates an HTTP error response with the specified status code and message.
        /// </summary>
        /// <param name="req">The original HTTP request that the response is associated with.</param>
        /// <param name="statusCode">The HTTP status code for the error response.</param>
        /// <param name="message">The error message to include in the response body.</param>
        /// <returns>An <see cref="HttpResponseData"/> representing the error response.</returns>
        public static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
        {
            // Create a response with the specified status code
            var response = req.CreateResponse(statusCode);

            // Write the error message to the response body and wait for the operation to complete
            response.WriteStringAsync(message).Wait(); // Blocking wait for simplicity in synchronous context

            // Return the constructed error response
            return response;
        }
    }
}

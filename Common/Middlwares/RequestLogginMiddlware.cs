namespace Dirassati_Backend.Common.Middlwares

{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request details
            var requestId = Guid.NewGuid().ToString();

            // Log request method, path, and query
            _logger.LogInformation(
                "Request {RequestId} received: {Method} {Path}{QueryString}",
                requestId, context.Request.Method, context.Request.Path, context.Request.QueryString);

            // Log important headers including Authorization (but mask token details)
            await LogHeaders(context.Request.Headers, requestId);

            // Enable buffering so we can read the response body
            context.Response.OnStarting(() =>
            {
                // Log response status code
                _logger.LogInformation(
                    "Request {RequestId} completed with status code {StatusCode}",
                    requestId, context.Response.StatusCode);
                return Task.CompletedTask;
            });

            // Continue processing the HTTP request
            await _next(context);
        }

        private Task LogHeaders(IHeaderDictionary headers, string requestId)
        {
            foreach (var header in headers)
            {
                // Special handling for Authorization header to prevent exposing full tokens
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    var authValue = header.Value.ToString();
                    var maskedValue = MaskAuthorizationHeader(authValue);
                    _logger.LogInformation("Request {RequestId} Header: {HeaderName}: {HeaderValue}",
                        requestId, header.Key, maskedValue);
                }
                else if (header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    // Log that cookies were present but don't log the values
                    _logger.LogInformation("Request {RequestId} Header: {HeaderName}: [Cookies present]",
                        requestId, header.Key);
                }
                else
                {
                    _logger.LogInformation("Request {RequestId} Header: {HeaderName}: {HeaderValue}",
                        requestId, header.Key, header.Value);
                }
            }

            return Task.CompletedTask;
        }

        private string MaskAuthorizationHeader(string authHeader)
        {
            // Mask token details but leave scheme visible
            if (string.IsNullOrEmpty(authHeader)) return string.Empty;

            var parts = authHeader.Split(' ', 2);
            if (parts.Length > 1)
            {
                // Return just the auth scheme and a masked token
                string scheme = parts[0]; // e.g., Bearer, Basic, etc.
                return $"{scheme} [TOKEN MASKED]";
            }

            return "[CREDENTIAL MASKED]";
        }
    }
}
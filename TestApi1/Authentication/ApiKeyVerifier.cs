namespace TestApi1.Authentication
{
    public class ApiKeyVerifier
    {
        private readonly RequestDelegate _next;
        public ApiKeyVerifier(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out
                    var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided ");
                return;
            }
            var keyValidation = context.RequestServices.GetRequiredService<IApiKeyValidation>();
            if (!keyValidation.IsValidApiKey(extractedApiKey!, out var clientId, out var clientName))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client");
                return;
            }
            else
            {
                context.Request.Headers.Append(Constants.ApiClientIdHederName, clientId.ToString());
                context.Request.Headers.Append(Constants.ApiClientHederName, clientName);
            }
            await _next(context);
        }
    }
}

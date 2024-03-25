using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApi1.Authentication;

namespace TestApi1UnitTests
{
    /// <summary>
    /// Middleware simulator used in tests to be able to test the API
    /// </summary>
    public class TestKeyVerifier
    {
        private readonly IApiKeyValidation _validator;
        public TestKeyVerifier(IApiKeyValidation validator)
        {
            _validator = validator;
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
            if (!_validator.IsValidApiKey(extractedApiKey!, out var clientId, out var clientName))
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
        }
    }
}

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Basic_HTTP_Trigger
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string name = null;

            // Try to get name from query parameters
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            name = query["name"];

            // If name not found in query, try to get it from JSON body
            if (string.IsNullOrEmpty(name))
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        var json = JsonDocument.Parse(body);
                        if (json.RootElement.TryGetProperty("name", out JsonElement nameElement))
                        {
                            name = nameElement.GetString();
                        }
                    }
                    catch { }
                }
            }

            name ??= "User";

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync($"Hello, \"{name}\"! Your Azure Function executed successfully.");
            return response;
        }
    }
}

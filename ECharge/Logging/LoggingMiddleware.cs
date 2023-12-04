using System.Text;
using System.Text.Json;
using ECharge.Domain.Entities;
using ECharge.Infrastructure.Services.DatabaseContext;

namespace ECharge.Api.Logging
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        private async Task<string> GetRequestBodyAsStringAsync(HttpRequest request)
        {
            request.EnableBuffering();

            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task Invoke(HttpContext context)
        {
            using (var dbContext = new DataContext())
            {
                string requestBodyText = string.Empty;
                string responseBodyText = string.Empty;
                string error = string.Empty;

                requestBodyText = await GetRequestBodyAsStringAsync(context.Request);

                var requestBodyStream = new MemoryStream(Encoding.UTF8.GetBytes(requestBodyText));
                context.Request.Body = requestBodyStream;

                using (var memoryStream = new MemoryStream())
                {
                    var originalResponseBody = context.Response.Body;
                    context.Response.Body = memoryStream;

                    try
                    {
                        await _next(context);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                        context.Response.StatusCode = 500;
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalResponseBody);
                    responseBodyText = Encoding.UTF8.GetString(memoryStream.ToArray());
                    context.Response.Body = originalResponseBody;
                }

                context.Request.Body = requestBodyStream;

                var filteredContext = new FilteredContext
                {
                    Timestamp = DateTime.Now,
                    Request = new RequestInfo
                    {
                        Path = context.Request.Path,
                        Method = context.Request.Method,
                        QueryString = context.Request.QueryString,
                        QueryKeyValuePairs = context.Request.Query,
                        Headers = context.Request.Headers,
                        Body = requestBodyText
                    },
                    Response = new ResponseInfo
                    {
                        StatusCode = context.Response.StatusCode,
                        Headers = context.Response.Headers,
                        Body = responseBodyText,
                        Error = error
                    }
                };

                if (!(context.Request.Path == "/api/echarge/get-session-status" && context.Response.StatusCode == 200))
                {
                    string jsonContext = JsonSerializer.Serialize(filteredContext);

                    await dbContext.LogEntries.AddAsync(new LogEntry
                    {
                        RequestResponseDetails = jsonContext,
                        Timestamp = DateTime.Now
                    });

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}


using ECharge.Domain.HttpRequest.Interface;
using ECharge.Domain.HttpRequest.Model;
using Newtonsoft.Json;
using System.Net;

namespace ECharge.Infrastructure.Services.HttpRequest
{
    public class HttpRequestHandler : IHttpRequestHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpRequestHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpRequestHandlerResponse<TResponseObject>> SendRequest<TResponseObject>(HttpRequestMessage request)
        {
            try
            {
                HttpClient httpClient = _httpClientFactory.CreateClient();

                HttpResponseMessage response = await httpClient.SendAsync(request);

                var jsonDataResponse = await response.Content.ReadAsStringAsync();

                TResponseObject responseObject = JsonConvert.DeserializeObject<TResponseObject>(jsonDataResponse);

                var handlerResponse = new HttpRequestHandlerResponse<TResponseObject>(response.StatusCode, responseObject);

                return handlerResponse;
            }
            catch (HttpRequestException)
            {
                return new HttpRequestHandlerResponse<TResponseObject> { HttpStatus = HttpStatusCode.InternalServerError };
            }
        }
    }
}


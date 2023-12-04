using System.Net.Http.Headers;
using System.Text;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model;
using Newtonsoft.Json;
using Microsoft.AspNetCore.WebUtilities;
using ECharge.Domain.CibPay.Model.CreateOrder.Command;
using ECharge.Domain.CibPay.Model.CreateOrder.Response;
using System.Security.Cryptography.X509Certificates;
using ECharge.Domain.CibPay.Model.Ping.Response;
using ECharge.Domain.CibPay.Model.RefundOrder.Command;
using ECharge.Domain.CibPay.Model.BaseResponse;
using ECharge.Infrastructure.Services.CibPay.Certificate.Api;
using Microsoft.Extensions.Configuration;
using ECharge.Domain.CibPay.Model.RefundOrder.Response;

namespace ECharge.Infrastructure.Services.CibPay.Service
{
    public class CibPayService : ICibPayService
    {
        private readonly HttpClient _httpClient;
        private readonly X509Certificate2 _clientCertificate;
        private readonly string _credentials;
        private readonly string _paymentUrl;
        private readonly string username;
        private readonly string password;
        private readonly string _returnUrl;
        private readonly bool _autoCharge;
        private readonly byte _force3D;
        private readonly string _currency;
        private readonly string _expirationTimeout;
        private readonly string _language;

        public CibPayService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            username = configuration["CibPay:Username"];
            password = configuration["CibPay:Password"];
            _paymentUrl = configuration["CibPay:BaseUrl"];
            _returnUrl = configuration["CibPay:ReturnUrl"];

            _autoCharge = bool.Parse(configuration["CibPay:AutoCharge"]);
            _force3D = byte.Parse(configuration["CibPay:Force3D"]);
            _currency = configuration["CibPay:Currency"];
            _expirationTimeout = configuration["CibPay:ExpirationTimeout"];
            _language = configuration["CibPay:Language"];
            _clientCertificate = GetCertificate();
            _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.BaseAddress = new Uri(_paymentUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
        }

        private async Task<CibBaseResponse<T>> SendRequestAsync<T>(string endpoint, HttpMethod method, object requestData = null)
        {
            using var handler = new HttpClientHandler { ClientCertificates = { _clientCertificate } };

            using var httpClientWithCertificate = new HttpClient(handler);
            httpClientWithCertificate.BaseAddress = _httpClient.BaseAddress;
            httpClientWithCertificate.DefaultRequestHeaders.Authorization = _httpClient.DefaultRequestHeaders.Authorization;

            using var request = new HttpRequestMessage(method, endpoint);

            var response = new CibBaseResponse<T>();

            if (requestData is not null)
            {
                var jsonContent = JsonConvert.SerializeObject(requestData);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            using var providerResponse = await httpClientWithCertificate.SendAsync(request);

            response.StatusCode = providerResponse.StatusCode;

            var stringResponseContent = await providerResponse.Content.ReadAsStringAsync();

            if (providerResponse.IsSuccessStatusCode)
            {
                response.Data = JsonConvert.DeserializeObject<T>(stringResponseContent);
            }
            else
            {
                var deserializedResponseContent = JsonConvert.DeserializeObject<CibBaseResponse>(stringResponseContent);

                response.FailureMessage = deserializedResponseContent.FailureMessage;
                response.FailureType = deserializedResponseContent.FailureType;
                response.OrderId = deserializedResponseContent.OrderId;
            }


            return response;
        }

        public async Task<CibBaseResponse<CreateOrderProviderResponse>> CreateOrder(CreateOrderCommand command)
        {
            try
            {
                var endpoint = "orders/create";

                var requestData = new
                {
                    amount = command.Amount,
                    currency = _currency,
                    custom_fields = new { charge_point_id = command.ChargePointId },
                    merchant_order_id = command.MerchantOrderId,
                    options = new
                    {
                        auto_charge = _autoCharge,
                        expiration_timeout = _expirationTimeout,
                        force3d = _force3D,
                        language = _language,
                        return_url = _returnUrl
                    },
                    client = new
                    {
                        name = "Optima Group CO",
                        email = "info@optima.az"
                    }
                };

                return await SendRequestAsync<CreateOrderProviderResponse>(endpoint, HttpMethod.Post, requestData);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        public async Task<CibBaseResponse<AllOrdersResponse>> GetOrderInfo(string orderId)
        {
            try
            {
                var endpoint = $"orders/{orderId}";
                return await SendRequestAsync<AllOrdersResponse>(endpoint, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        public async Task<CibBaseResponse<AllOrdersResponse>> GetOrdersList(GetOrdersQuery query)
        {
            try
            {
                var endpoint = "orders/";
                var queryParameters = new Dictionary<string, string>
                {
                    ["status"] = query.Status,
                    ["created_from"] = query.CreatedFrom?.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["created_to"] = query.CreatedTo?.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["merchant_order_id"] = query.MerchantOrderId,
                    ["card.type"] = query.CardType,
                    ["card.subtype"] = query.CardSubtype,
                    ["location.ip"] = query.IpAddress,
                    ["expand"] = query.Expand,
                    ["page"] = query.PageIndex.ToString(),
                    ["page_size"] = query.PageSize.ToString()
                };

                endpoint = QueryHelpers.AddQueryString(endpoint, queryParameters);

                return await SendRequestAsync<AllOrdersResponse>(endpoint, HttpMethod.Get); ;
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        public async Task<CibBaseResponse<PingResponse>> GetPingResponse()
        {
            try
            {
                var endpoint = "ping";
                return await SendRequestAsync<PingResponse>(endpoint, HttpMethod.Get);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        public async Task<CibBaseResponse<RefundOrderResponse>> RefundOrder(RefundOrderCommand refundOrderCommand)
        {
            try
            {
                var endpoint = $"orders/{refundOrderCommand.OrderId}/refund";

                var requestData = new { };

                return await SendRequestAsync<RefundOrderResponse>(endpoint, HttpMethod.Put, requestData);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        public async Task<CibBaseResponse<RefundOrderResponse>> RefundSpecificAmount(RefundSpecificAmoundOrderCommand command)
        {
            try
            {
                var endpoint = $"orders/{command.OrderId}/refund";

                var requestData = new
                {
                    amount = command.RefundAmount,
                };

                return await SendRequestAsync<RefundOrderResponse>(endpoint, HttpMethod.Put, requestData);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        private X509Certificate2 GetCertificate()
        {
            var cPath = new CertificatePath();

            return new X509Certificate2(cPath.CurrentPath, "vakE1quIXIZc");
        }
    }
}

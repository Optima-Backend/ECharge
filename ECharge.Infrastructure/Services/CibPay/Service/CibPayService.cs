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
using static System.Net.WebRequestMethods;

namespace ECharge.Infrastructure.Services.CibPay.Service
{
    public class CibPayService : ICibPayService
    {
        private readonly HttpClient _httpClient;
        private readonly X509Certificate2 _clientCertificate;
        private readonly string _credentials;
        private readonly string _paymentUrl;

        public CibPayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            var username = "taxiapp";
            var password = "n6XufZfZoWjjwjZMHq";
            _paymentUrl = "https://checkout-preprod.cibpay.co/pay/";
            _clientCertificate = GetCertificate();
            _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.BaseAddress = new Uri("https://api-preprod.cibpay.co");
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
                    currency = string.IsNullOrEmpty(command.Currency) ? "AZN" : command.Currency,
                    extra_fields = new { invoice_id = string.IsNullOrEmpty(command.InvoiceId) ? "PKgn75jj2e2RDCkB" : command.InvoiceId },
                    merchant_order_id = string.IsNullOrEmpty(command.MerchantOrderId) ? "PKgn75jj2e2RDCkB" : command.MerchantOrderId,
                    options = new
                    {
                        auto_charge = command.AutoCharge ?? true,
                        expiration_timeout = string.IsNullOrEmpty(command.ExpirationTimeout) ? "4320m" : command.ExpirationTimeout,
                        force3d = command.Force3d ?? 1,
                        language = string.IsNullOrEmpty(command.Language) ? "az" : command.Language,
                        //http://4.193.153.177:4444/api/echarge/payment-redirect-url
                        return_url = string.IsNullOrEmpty(command.ReturnUrl) ? "https://google.com" : command.ReturnUrl
                    },
                    client = new
                    {
                        name = string.IsNullOrEmpty(command.Name) ? "Orkhan" : command.Name,
                        email = string.IsNullOrEmpty(command.Email) ? "amirovorxan@gmail.com" : command.Email
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
                    ["expand"] = query.Expand
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

        public async Task<CibBaseResponse<SingleOrderResponse>> RefundOrder(RefundOrderCommand refundOrderCommand)
        {
            try
            {
                var endpoint = $"orders/{refundOrderCommand.OrderId}/refund";

                var requestData = new
                {
                    amount = refundOrderCommand.RefundAmount,
                };

                return await SendRequestAsync<SingleOrderResponse>(endpoint, HttpMethod.Put, requestData);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        private X509Certificate2 GetCertificate()
        {
            string cPath = new CertificatePath().CurrentPath;

            return new X509Certificate2(cPath, "nBR2SFVWZ02g");
        }
    }
}

using ECharge.Domain.GatewayApiHelper.Interface;
using System.Net.Http.Headers;
using ECharge.Domain.GatewayApiHelper.Model;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace ECharge.Infrastructure.Services.GatewayApiHelper
{
    public class GatewayApiService : IGatewayApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _credentials;

        public GatewayApiService(string baseUrl, string credentials)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _baseUrl = baseUrl;
            _credentials = credentials;
        }

        public async Task<PingResponse> GetPingResponse()
        {
            string endpoint = "ping";

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<PingResponse>(responseBody);
                    return responseObject;
                }
                else
                {
                    return new PingResponse
                    {
                        Date = null,
                        Message = $"Request failed with status code: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new PingResponse
                {
                    Date = null,
                    Message = $"HTTP request error: {ex.Message}"
                };
            }
        }

        public async Task<OrderInfoModel> GetOrderInfo(string orderId)
        {
            string endpoint = $"orders/{orderId}";

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderInfoModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderListModel> GetOrdersList(string status = null, DateTime? createdFrom = null, DateTime? createdTo = null,
            string merchantOrderId = null, string cardType = null, string cardSubtype = null, string ipAddress = null,
            string expand = null)
        {
            string endpoint = "orders";

            var queryParameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(status))
                queryParameters.Add("status", status);
            if (createdFrom.HasValue)
                queryParameters.Add("created_from", createdFrom.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (createdTo.HasValue)
                queryParameters.Add("created_to", createdTo.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (!string.IsNullOrEmpty(merchantOrderId))
                queryParameters.Add("merchant_order_id", merchantOrderId);
            if (!string.IsNullOrEmpty(cardType))
                queryParameters.Add("card.type", cardType);
            if (!string.IsNullOrEmpty(cardSubtype))
                queryParameters.Add("card.subtype", cardSubtype);
            if (!string.IsNullOrEmpty(ipAddress))
                queryParameters.Add("location.ip", ipAddress);
            if (!string.IsNullOrEmpty(expand))
                queryParameters.Add("expand", expand);

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                // Append query parameters to the endpoint URL
                endpoint = QueryHelpers.AddQueryString(endpoint, queryParameters);

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderListModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OperationListModel> GetOperationsList(string expand = null, string status = null, string type = null,
            DateTime? createdFrom = null, DateTime? createdTo = null)
        {
            string endpoint = "operations";

            var queryParameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(expand))
                queryParameters.Add("expand", expand);
            if (!string.IsNullOrEmpty(status))
                queryParameters.Add("status", status);
            if (!string.IsNullOrEmpty(type))
                queryParameters.Add("type", type);
            if (createdFrom.HasValue)
                queryParameters.Add("created_from", createdFrom.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (createdTo.HasValue)
                queryParameters.Add("created_to", createdTo.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                // Append query parameters to the endpoint URL
                endpoint = QueryHelpers.AddQueryString(endpoint, queryParameters);

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OperationListModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<ExchangeRateModel> GetExchangeRates(DateTime? date = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            string endpoint = "exchange_rates";

            // Prepare the query parameters
            var queryParameters = new Dictionary<string, string>();
            if (date.HasValue)
                queryParameters.Add("date", date.Value.ToString("yyyy-MM-dd"));
            if (dateFrom.HasValue)
                queryParameters.Add("date_from", dateFrom.Value.ToString("yyyy-MM-dd"));
            if (dateTo.HasValue)
                queryParameters.Add("date_to", dateTo.Value.ToString("yyyy-MM-dd"));

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                // Append query parameters to the endpoint URL
                endpoint = QueryHelpers.AddQueryString(endpoint, queryParameters);

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ExchangeRateModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderCreateResponseModel> CreateOrder(decimal amount, string currency = "USD", string merchantOrderId = null,
            string segment = null)
        {
            string endpoint = "orders/create";

            var requestData = new
            {
                amount = amount,
                currency = currency,
                merchant_order_id = merchantOrderId,
                segment = segment
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode && response.Headers.Location != null)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderCreateResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderAuthorizeResponseModel> AuthorizeOrder(decimal amount, string pan, CardModel card,
            LocationInfoModel location = null, string currency = "USD", string merchantOrderId = null,
            string description = null, string clientAddress = null, string clientCity = null, string clientCountry = null,
            string clientEmail = null, string clientName = null, string clientPhone = null, string clientState = null,
            string clientZip = null, int? force3d = null, string returnUrl = null, int? autoCharge = null,
            string terminal = null, int? recurring = null, string secure3d20ReturnUrl = null, int? exemptionMit = null)
        {
            string endpoint = "orders/authorize";

            var requestData = new
            {
                amount = amount,
                card = card,
                location = location,
                currency = currency,
                merchant_order_id = merchantOrderId,
                description = description,
                client = new
                {
                    address = clientAddress,
                    city = clientCity,
                    country = clientCountry,
                    email = clientEmail,
                    name = clientName,
                    phone = clientPhone,
                    state = clientState,
                    zip = clientZip
                },
                options = new
                {
                    force3d = force3d,
                    return_url = returnUrl,
                    auto_charge = autoCharge,
                    terminal = terminal,
                    recurring = recurring,
                    secure3d20_return_url = secure3d20ReturnUrl,
                    exemption_mit = exemptionMit
                },
                pan = pan
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderAuthorizeResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderReverseResponseModel> ReverseOrder(string orderId)
        {
            string endpoint = $"orders/{orderId}/reverse";

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PutAsync(endpoint, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderReverseResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderChargeResponseModel> ChargeOrder(string orderId, decimal chargeAmount)
        {
            string endpoint = $"orders/{orderId}/charge";

            var requestData = new { amount = chargeAmount };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderChargeResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderRefundResponseModel> RefundOrder(string orderId, decimal refundAmount)
        {
            string endpoint = $"orders/{orderId}/refund";

            var requestData = new { amount = refundAmount };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderRefundResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<OrderCancelResponseModel> CancelOrder(string orderId, decimal? refundAmount = null)
        {
            string endpoint = $"orders/{orderId}/cancel";

            var requestData = new { amount = refundAmount };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PutAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderCancelResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<RebillResponseModel> PerformRebill(string orderId, decimal amount, string cvv, string clientIp, bool recurring = false)
        {
            string endpoint = $"orders/{orderId}/rebill";

            var requestData = new
            {
                amount = amount,
                cvv = cvv,
                location = new { ip = clientIp },
                options = new { recurring = recurring }
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<RebillResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<CreditResponseModel> PerformCredit(string orderId, decimal amount, string currency, string clientIp)
        {
            string endpoint = $"orders/{orderId}/credit";

            var requestData = new
            {
                amount = amount,
                currency = currency,
                location = new { ip = clientIp }
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<CreditResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<CreditResponseModel> PerformOriginalCredit(decimal amount, string pan)
        {
            string endpoint = "orders/credit";

            var requestData = new
            {
                amount = amount,
                pan = pan
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<CreditResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<CreditResponseModel> PerformOriginalCreditWithoutLink(decimal amount, string pan)
        {
            string endpoint = "orders/credit";

            var requestData = new
            {
                amount = amount,
                pan = pan
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<CreditResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<CompleteResponseModel> Complete3DSecureAuthentication(string orderId, string paRes)
        {
            string endpoint = $"orders/{orderId}/complete";

            var requestData = new
            {
                PaRes = paRes
            };

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<CompleteResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<Complete3D20ResponseModel> Complete3D20Authentication(string orderId)
        {
            string endpoint = $"orders/{orderId}/complete3d20";

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Complete3D20ResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<Resume3DSResponseModel> Resume3DSAuthentication(string orderId)
        {
            string endpoint = $"orders/{orderId}/resume";

            try
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Resume3DSResponseModel>(responseBody);
                    return responseObject;
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}



using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.DTOs.Responses;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.EVtrip.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;


namespace ECharge.Infrastructure.Services.EVtrip
{
    public class ChargePointApiClient : IChargePointApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://admin.evtrip.net";
        private const string TokenEndpoint = "/api/oauth/token";
        private const string ClientId = "82783";
        private const string ClientSecret = "yzgPOubhxBL9";

        public ChargePointApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<OperationResult<string>> GetAccessTokenAsync()
        {
            OperationResult<string> tokenResult = new();

            Dictionary<string, string> formData = new()
            {
                { "grant_type", "password" },
                { "username", "ilab-api" },
                { "password", "111" },
                { "client_id", ClientId },
                { "client_secret", ClientSecret }
            };

            var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(formData));

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                tokenResult.Success = false;
                tokenResult.Error!.Code = "500";
                tokenResult.Error.Message = "Token can not be generated";
                tokenResult.Result = "";
            }

            tokenResult.Result = JsonConvert.DeserializeObject<AccessTokenModel>(result).AccessToken;
            tokenResult.Success = true;

            return tokenResult;
        }

        private async Task<HttpClient> GetAuthorizedHttpClientAsync()
        {
            var accessToken = await GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.Result);

            return _httpClient;

        }

        public async Task<IQueryable<ChargePointShortView>> GetAllChargePointsAsync()
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();

                HttpResponseMessage response = await authorizedClient.GetAsync("/api/external/cpo/v1/chargepoint/");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var chargePoints = JsonConvert.DeserializeObject<List<ChargePointShortView>>(content);
                return chargePoints.AsQueryable();
            }
            catch (HttpRequestException ex)
            {

                throw new Exception("API isteği başarısız oldu: " + ex.Message);
            }
        }

        public async Task<ChargingSession> GetChargingSessionsAsync(string chargepointId)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();

                HttpResponseMessage response =
                    await authorizedClient.GetAsync(
                        $"/api/external/cpo/v1/chargepoint/{chargepointId}/sessions/current");

                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                var chargingSessions = JsonConvert.DeserializeObject<ChargingSession>(content);
                return chargingSessions;
            }
            catch (HttpRequestException ex)
            {
                //TODO - Log və exception'ları burada handle etmək!
                throw new Exception("API isteği başarısız oldu: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargePoint>> GetSingleChargerAsync(string chargepointId)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();

                OperationResult<ChargePoint> singlePointResult = new();

                HttpResponseMessage response =
                    await authorizedClient.GetAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}");

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    singlePointResult.Result = null;
                    singlePointResult.Success = false;
                    singlePointResult.Error!.Code = "500";
                    singlePointResult.Error.Message = $"Can't get information for id: {chargepointId}";

                }

                singlePointResult.Result = JsonConvert.DeserializeObject<ChargePoint>(result);
                singlePointResult.Success = true;
                singlePointResult.Error = null;

                return singlePointResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public async Task<OperationResult<ChargingSession>> StartChargingAsync(string chargepointId,
            StartChargingRequest request)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();

                string requestJson = JsonConvert.SerializeObject(request);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response =
                    await authorizedClient.PostAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}/start",
                        content);

                response.EnsureSuccessStatusCode();

                string responseJson = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<OperationResult<ChargingSession>>(responseJson);

                return result;
            }
            catch (HttpRequestException ex)
            {
                //TODO - Log və exception'ları burada handle etmək!
                throw new Exception("API isteği başarısız oldu: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargingSession>> StopChargingAsync(string chargepointId,
            StopChargingRequest request)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();

                string requestJson = JsonConvert.SerializeObject(request);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response =
                    await authorizedClient.PostAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}/stop", content);

                string responseJson = await response.Content.ReadAsStringAsync();


                var result = JsonConvert.DeserializeObject<OperationResult<ChargingSession>>(responseJson);

                return result;
            }
            catch (HttpRequestException ex)
            {
                //TODO - Log və exception'ları burada handle etmək!
                throw new Exception("API isteği başarısız oldu: " + ex.Message);
            }
        }

    }

}


using System.Net.Http.Headers;
using System.Text;
using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.DTOs.Responses;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.EVtrip.Models;
using Newtonsoft.Json;

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

        private async Task<string> GetAccessTokenAsync()
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "ilab-api"),
                new KeyValuePair<string, string>("password", "111"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret)
            };

            var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(formData));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to obtain access token: " + response.StatusCode + " " + responseContent);
            }

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            return tokenResponse.AccessToken;
        }

        private async Task<HttpClient> GetAuthorizedHttpClientAsync()
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return _httpClient;
        }

        public async Task<List<ChargePointShortView>> GetAllChargePointsAsync()
        {
            try
            {
                using (var authorizedHttpClient = await GetAuthorizedHttpClientAsync())
                {
                    HttpResponseMessage response = await authorizedHttpClient.GetAsync("/api/external/cpo/v1/chargepoint/");
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();
                    var chargePoints = JsonConvert.DeserializeObject<List<ChargePointShortView>>(content);
                    return chargePoints;
                }
            }
            catch (HttpRequestException ex)
            {
                // TODO - Handle exceptions and logging
                throw new Exception("API request failed: " + ex.Message);
            }
        }

        public async Task<List<ChargingSession>> GetChargingSessionsAsync(string chargepointId)
        {
            try
            {
                using (var authorizedHttpClient = await GetAuthorizedHttpClientAsync())
                {
                    HttpResponseMessage response = await authorizedHttpClient.GetAsync($"/external/cpo/v1/chargepoint/{chargepointId}/sessions");
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();
                    var chargingSessions = JsonConvert.DeserializeObject<List<ChargingSession>>(content);
                    return chargingSessions;
                }
            }
            catch (HttpRequestException ex)
            {
                // TODO - Handle exceptions and logging
                throw new Exception("API request failed: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargingSession>> StartChargingAsync(string chargepointId, StartChargingRequest request)
        {
            try
            {
                using (var authorizedHttpClient = await GetAuthorizedHttpClientAsync())
                {
                    string requestJson = JsonConvert.SerializeObject(request);
                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await authorizedHttpClient.PostAsync($"/external/cpo/v1/chargepoint/{chargepointId}/start", content);
                    response.EnsureSuccessStatusCode();

                    string responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OperationResult<ChargingSession>>(responseJson);
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                // TODO - Handle exceptions and logging
                throw new Exception("API request failed: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargingSession>> StopChargingAsync(string chargepointId, StopChargingRequest request)
        {
            try
            {
                using (var authorizedHttpClient = await GetAuthorizedHttpClientAsync())
                {
                    string requestJson = JsonConvert.SerializeObject(request);
                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await authorizedHttpClient.PostAsync($"/external/cpo/v1/chargepoint/{chargepointId}/stop", content);
                    response.EnsureSuccessStatusCode();

                    string responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OperationResult<ChargingSession>>(responseJson);
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                // TODO - Handle exceptions and logging
                throw new Exception("API request failed: " + ex.Message);
            }
        }

    }
}

using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.DTOs.Responses;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.EVtrip.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ECharge.Infrastructure.Services.EVtrip
{
    public class ChargePointApiClient : IChargePointApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string BaseUrl;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string Username;
        private readonly string Password;

        private AccessTokenModel _currentToken;
        private DateTime _tokenExpiryTime;

        public ChargePointApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            BaseUrl = configuration["EVTrip:BaseURl"];
            ClientId = configuration["EVTrip:ClientId"];
            ClientSecret = configuration["EVTrip:ClientSecret"];
            Username = configuration["EVTrip:Username"];
            Password = configuration["EVTrip:Password"];

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<AccessTokenModel> GetAccessTokenAsync()
        {
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", Username },
                { "password", Password },
                { "client_id", ClientId },
                { "client_secret", ClientSecret }
            };

            var response = await _httpClient.PostAsync("/api/oauth/token", new FormUrlEncodedContent(formData));
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccessTokenModel>(result);
        }

        private async Task<AccessTokenModel> GetOrRefreshTokenAsync()
        {
            if (_currentToken == null || DateTime.UtcNow >= _tokenExpiryTime)
            {
                _currentToken = await GetAccessTokenAsync();
                _tokenExpiryTime = DateTime.UtcNow.AddSeconds(_currentToken.ExpiresIn - 10); // Refresh token 10 seconds before expiry
            }

            return _currentToken;
        }

        private async Task<HttpClient> GetAuthorizedHttpClientAsync()
        {
            var accessToken = await GetOrRefreshTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
            return _httpClient;
        }

        public async Task<IQueryable<ChargePointShortView>> GetAllChargePointsAsync()
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();
                var response = await authorizedClient.GetAsync("/api/external/cpo/v1/chargepoint/");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var chargePoints = JsonConvert.DeserializeObject<List<ChargePointShortView>>(content);
                return chargePoints.AsQueryable();
            }
            catch (Exception ex)
            {
                throw new Exception("EV-Trip API request failed: " + ex.Message);
            }
        }

        public async Task<ChargingSession> GetChargingSessionsAsync(string chargepointId)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();
                var response = await authorizedClient.GetAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}/sessions/current");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChargingSession>(content);
            }
            catch (Exception ex)
            {
                throw new Exception("EV-Trip API request failed: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargePoint>> GetSingleChargerAsync(string chargepointId)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();
                var response = await authorizedClient.GetAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return new OperationResult<ChargePoint>
                    {
                        Result = JsonConvert.DeserializeObject<ChargePoint>(result),
                        Success = true
                    };
                }
                else
                {
                    return new OperationResult<ChargePoint>
                    {
                        Success = false,
                        Error = new Error
                        {
                            Code = ((int)response.StatusCode).ToString(),
                            Message = $"Can't get information for id: {chargepointId}"
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EV-Trip API request failed: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargingSession>> StartChargingAsync(string chargepointId, StartChargingRequest request)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();
                var requestJson = JsonConvert.SerializeObject(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await authorizedClient.PostAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}/start", content);

                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OperationResult<ChargingSession>>(responseJson);
            }
            catch (Exception ex)
            {
                throw new Exception("EV-Trip API request failed: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargingSession>> StopChargingAsync(string chargepointId, StopChargingRequest request)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();
                var requestJson = JsonConvert.SerializeObject(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await authorizedClient.PostAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}/stop", content);

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OperationResult<ChargingSession>>(responseJson);
            }
            catch (Exception ex)
            {
                throw new Exception("EV-Trip API request failed: " + ex.Message);
            }
        }

        public async Task<List<ChargingSession>> GetSingleChargePointSessions(string chargepointId)
        {
            try
            {
                var authorizedClient = await GetAuthorizedHttpClientAsync();
                var response = await authorizedClient.GetAsync($"/api/external/cpo/v1/chargepoint/{chargepointId}/sessions");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ChargingSession>>(content);
            }
            catch (Exception ex)
            {
                throw new Exception("EV-Trip API request failed: " + ex.Message);
            }
        }
    }
}



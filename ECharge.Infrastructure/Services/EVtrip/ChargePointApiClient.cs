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
        private const string BaseUrl = "https://localhost"; //TODO - Api Root addresini dəyişdirmək!

        public ChargePointApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<ChargePoint>> GetAllChargePointsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("/external/cpo/v1/chargepoint/");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var chargePoints = JsonConvert.DeserializeObject<List<ChargePoint>>(content);
                return chargePoints;
            }
            catch (HttpRequestException ex)
            {
                //TODO - Log və exception'ları burada handle etmək!
                throw new Exception("API isteği başarısız oldu: " + ex.Message);
            }
        }

        public async Task<List<ChargingSession>> GetChargingSessionsAsync(string chargepointId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/external/cpo/v1/chargepoint/{chargepointId}/sessions");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var chargingSessions = JsonConvert.DeserializeObject<List<ChargingSession>>(content);
                return chargingSessions;
            }
            catch (HttpRequestException ex)
            {
                //TODO - Log və exception'ları burada handle etmək!
                throw new Exception("API isteği başarısız oldu: " + ex.Message);
            }
        }

        public async Task<OperationResult<ChargingSession>> StartChargingAsync(string chargepointId, StartChargingRequest request)
        {
            try
            {
                string requestJson = JsonConvert.SerializeObject(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"/external/cpo/v1/chargepoint/{chargepointId}/start", content);
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

        public async Task<OperationResult<ChargingSession>> StopChargingAsync(string chargepointId, StopChargingRequest request)
        {
            try
            {
                string requestJson = JsonConvert.SerializeObject(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"/external/cpo/v1/chargepoint/{chargepointId}/stop", content);
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

    }

}


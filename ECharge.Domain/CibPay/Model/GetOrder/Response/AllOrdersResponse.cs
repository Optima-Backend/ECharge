using Newtonsoft.Json;

namespace ECharge.Domain.CibPay.Model
{
    public class AllOrdersResponse
    {
        [JsonProperty("orders")]
        public List<SingleOrderResponse> Orders { get; set; }
    }
}


using System.Net;
using Newtonsoft.Json;

namespace ECharge.Domain.CibPay.Model.BaseResponse
{
    public class CibBaseResponse
    {
        [JsonProperty("failure_type")]
        public string FailureType { get; set; }

        [JsonProperty("failure_message")]
        public string FailureMessage { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }

    public class CibBaseResponse<T> : CibBaseResponse
    {
        public T Data { get; set; }
    }
}


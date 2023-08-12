using System;
namespace ECharge.Domain.GatewayApiHelper.Model
{
    public class OperationInfoModel
    {
        public decimal Amount { get; set; }
        public string AuthCode { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string IsoMessage { get; set; }
        public string IsoResponseCode { get; set; }
        public string OrderId { get; set; }
        public string Arn { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}


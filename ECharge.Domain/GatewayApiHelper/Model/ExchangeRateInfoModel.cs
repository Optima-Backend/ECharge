using System;
namespace ECharge.Domain.GatewayApiHelper.Model
{
    public class ExchangeRateInfoModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Rate { get; set; }
    }
}


using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ECharge.Infrastructure.Services.PulPal.Utils
{
    public class PulPalPayment
    {
        private const string Salt = "o(NPEn9fw8pU8vR6aQgkjro*1fu?.j)d";
        private const string MerchantIdProd = "3473";
        private const string MerchantIdDev = "520";
        private const string ApiUrlDev = "https://pay-dev.pulpal.az/payment";
        private const string ApiUrlProd = "https://pay.pulpal.az/payment";

        private string _merchantId;
        private int _amount;
        private bool _repeatable;
        private string _customMerchantName;
        private string _name_az;
        private string _name_ru;
        private string _name_en;
        private string _description_az;
        private string _description_ru;
        private string _description_en;
        private string _externalId;

        public bool IsDevelopment { get; set; } = false;

        public string GeneratePaymentUrl(int amountInCent, string productName, string description, string externalId)
        {
            _merchantId = IsDevelopment ? MerchantIdDev : MerchantIdProd;
            _amount = amountInCent;
            _repeatable = false;
            _customMerchantName = "E-Charge";
            _name_az = productName;
            _name_ru = productName;
            _name_en = productName;
            _description_az = description;
            _description_en = description;
            _description_ru = description;
            _externalId = externalId;

            string baseUrl = IsDevelopment ? ApiUrlDev : ApiUrlProd;
            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query["merchantId"] = _merchantId;
            query["price"] = _amount.ToString();
            query["repeatable"] = _repeatable.ToString();
            query["customMerchantName"] = _customMerchantName;
            query["name_az"] = _name_az;
            query["name_en"] = _name_en;
            query["name_ru"] = _name_ru;
            query["description_az"] = _description_az;
            query["description_en"] = _description_en;
            query["description_ru"] = _description_ru;
            query["externalId"] = _externalId;
            query["signature2"] = CalculateSignature();

            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        public static string GetDeliverySignature(string url, string nonce, string body)
        {
            using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(Salt));
            var buffer = Encoding.UTF8.GetBytes(url + nonce + body);
            hmacSha256.ComputeHash(buffer);

            return Convert.ToBase64String(hmacSha256.Hash);
        }

        private string CalculateSignature()
        {
            var microtime = DateTimeOffset.Now.ToUnixTimeSeconds();
            var time = microtime * 1000 / 300000;

            var builder = new StringBuilder();
            builder.Append(_name_en);
            builder.Append(_name_az);
            builder.Append(_name_ru);
            builder.Append(_description_en);
            builder.Append(_description_az);
            builder.Append(_description_ru);
            builder.Append(_merchantId);
            builder.Append(_externalId);
            builder.Append(_amount);
            builder.Append(time);
            builder.Append(Salt);

            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}


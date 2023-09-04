using Newtonsoft.Json;

namespace ECharge.Domain.CibPay.Model
{
    public class SingleOrderResponse
    {
        [JsonProperty("failure_message")]
        public string FailureMessage { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("amount_charged")]
        public decimal AmountCharged { get; set; }

        [JsonProperty("amount_refunded")]
        public decimal AmountRefunded { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("card")]
        public Card Card { get; set; }

        [JsonProperty("client")]
        public Client Client { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("custom_fields")]
        public Dictionary<string, object> CustomFields { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("issuer")]
        public Issuer Issuer { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("merchant_order_id")]
        public string MerchantOrderId { get; set; }

        [JsonProperty("operations")]
        public List<Operation> Operations { get; set; }

        [JsonProperty("pan")]
        public string Pan { get; set; }

        [JsonProperty("secure3d")]
        public Dictionary<string, object> Secure3d { get; set; }

        [JsonProperty("segment")]
        public string Segment { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }
    }

    public class Card
    {
        [JsonProperty("holder")]
        public string Holder { get; set; }

        [JsonProperty("subtype")]
        public string Subtype { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Client
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }
    }

    public class Issuer
    {
        [JsonProperty("bin")]
        public string Bin { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class Location
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
    }

    public class Cashflow
    {
        [JsonProperty("incoming")]
        public string Incoming { get; set; }

        [JsonProperty("fee")]
        public string Fee { get; set; }

        [JsonProperty("receivable")]
        public string Receivable { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reserve")]
        public string Reserve { get; set; }
    }

    public class Operation
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("iso_message")]
        public string IsoMessage { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("iso_response_code")]
        public string IsoResponseCode { get; set; }

        [JsonProperty("cashflow")]
        public Cashflow Cashflow { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("arn")]
        public string Arn { get; set; }
    }
}


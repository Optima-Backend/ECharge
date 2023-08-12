namespace ECharge.Domain.HttpRequest.Model
{
    public class HttpRequestHandlerHeader
    {
        public string Key;
        public string Value;

        public HttpRequestHandlerHeader(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}


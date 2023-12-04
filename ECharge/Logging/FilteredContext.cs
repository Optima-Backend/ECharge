namespace ECharge.Api.Logging
{
    public class FilteredContext
    {
        public DateTime Timestamp { get; set; }
        public RequestInfo Request { get; set; }
        public ResponseInfo Response { get; set; }
    }

    public class RequestInfo
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public QueryString QueryString { get; set; }
        public IQueryCollection QueryKeyValuePairs { get; set; }
        public object Headers { get; set; }
        public string Body { get; set; }
    }

    public class ResponseInfo
    {
        public int StatusCode { get; set; }
        public object Headers { get; set; }
        public string Body { get; set; }
        public string Error { get; set; }
    }
}


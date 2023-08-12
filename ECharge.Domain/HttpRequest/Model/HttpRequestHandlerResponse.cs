using System.Net;

namespace ECharge.Domain.HttpRequest.Model
{
    public class HttpRequestHandlerResponse<TResponseObject>
    {
        public HttpRequestHandlerResponse() { }

        public int HttpStatusCode { get; set; }
        public HttpStatusCode HttpStatus { get; set; }
        public TResponseObject Response { get; set; }

        public HttpRequestHandlerResponse(HttpStatusCode httpStatus, TResponseObject response = default)
        {
            HttpStatus = httpStatus;
            HttpStatusCode = (int)httpStatus;
            Response = response;
        }
    }
}


using ECharge.Domain.HttpRequest.Model;

namespace ECharge.Domain.HttpRequest.Interface
{
    public interface IHttpRequestHandler
    {
        Task<HttpRequestHandlerResponse<TResponseObject>> SendRequest<TResponseObject>(HttpRequestMessage request);
    }
}


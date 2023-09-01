using System.Net;

namespace ECharge.Domain.Repositories.Transaction.DTO
{
    public class BaseResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }

    public class BaseResponseWithPagination<T> : BaseResponse
    {
        public IQueryable<T> Data { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
        public int PageIndex { get; set; }


        public Task SetDataAsync(IQueryable<T> data, int pageIndex, int pageSize)
        {
            Data = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            PageSize = pageSize;
            TotalCount = data.Count();
            TotalPage = (int)Math.Ceiling(TotalCount / (double)pageSize);
            PageIndex = pageIndex;
            return Task.CompletedTask;
        }
    }
}


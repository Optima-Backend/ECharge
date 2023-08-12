namespace ECharge.Domain.EVtrip.DTOs.Responses
{
    public class OperationResult<T>
    {
        public Error? Error { get; set; }
        public T? Result { get; set; }
        public bool Success { get; set; }
    }
}


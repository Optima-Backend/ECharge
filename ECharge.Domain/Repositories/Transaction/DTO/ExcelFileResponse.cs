namespace ECharge.Domain.Repositories.Transaction.DTO;

public class ExcelFileResponse
{
    public string FileName { get; set; }
    public byte[] FileBytes { get; set; }
    public string ContentType { get; set; }
}
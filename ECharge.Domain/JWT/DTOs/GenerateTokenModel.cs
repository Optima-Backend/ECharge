namespace ECharge.Domain.JWT.DTOs
{
    public class GenerateTokenModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}


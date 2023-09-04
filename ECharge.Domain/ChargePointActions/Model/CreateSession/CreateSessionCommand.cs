namespace ECharge.Domain.ChargePointActions.Model.CreateSession
{
    public class CreateSessionCommand
    {
        public required TimeSpan Duration { get; set; }
        public required string ChargePointId { get; set; }
        public required string UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required decimal Price { get; set; }
    }
}


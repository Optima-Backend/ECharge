using AutoMapper;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.Mappings;

namespace ECharge.Domain.Repositories.Transaction.DTO
{
    public class AdminTransactionDTO : IMapFrom<Session>
    {
        public string Id { get; set; }

        public string ChargerPointId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public double DurationInMinutes { get; set; }

        public TimeSpan Duration { get; set; }

        public decimal PricePerHour { get; set; }

        public SessionStatus SessionStatus { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string ChargePointName { get; set; }

        public int? MaxVoltage { get; set; }

        public int? MaxAmperage { get; set; }

        public int OrderId { get; set; }

        public string ProviderOrderId { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public string Currency { get; set; }

        public decimal AmountCharged { get; set; }

        public PaymentStatus OrderStatus { get; set; }

        public string MerchantOrderId { get; set; }

        public decimal AmountRefunded { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public string Pan { get; set; }

        public DateTime OrderCreatedAt { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Session, AdminTransactionDTO>()
                .ForMember(x => x.SessionStatus, z => z.MapFrom(y => y.Status))
                .ForMember(x => x.OrderStatus, z => z.MapFrom(y => y.Order.Status))
                .ForMember(x => x.ProviderOrderId, z => z.MapFrom(y => y.Order.OrderId))
                .ForMember(x => x.Created, z => z.MapFrom(y => y.Order.Created))
                .ForMember(x => x.Updated, z => z.MapFrom(y => y.Order.Updated))
                .ForMember(x => x.Currency, z => z.MapFrom(y => y.Order.Currency))
                .ForMember(x => x.AmountCharged, z => z.MapFrom(y => y.Order.AmountCharged))
                .ForMember(x => x.MerchantOrderId, z => z.MapFrom(y => y.Order.MerchantOrderId))
                .ForMember(x => x.AmountRefunded, z => z.MapFrom(y => y.Order.AmountRefunded))
                .ForMember(x => x.Description, z => z.MapFrom(y => y.Order.Description))
                .ForMember(x => x.Amount, z => z.MapFrom(y => y.Order.Amount))
                .ForMember(x => x.Pan, z => z.MapFrom(y => y.Order.Pan))
                .ForMember(x => x.OrderCreatedAt, z => z.MapFrom(y => y.Order.OrderCreatedAt));
        }
    }


}


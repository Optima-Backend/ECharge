using AutoMapper;
using ECharge.Domain.Entities;
using ECharge.Domain.Mappings;

namespace ECharge.Domain.Repositories.Transaction.DTO;

public class OrderStatusChangedDTO : IMapFrom<OrderStatusChangedHook>
{
    public int Id { get; set; }
    public string ChargePointId { get; set; }
    public int Connector { get; set; }
    public string OrderUuid { get; set; }
    public string Status { get; set; }
    public string FinishReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SessionId { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrderStatusChangedHook, OrderStatusChangedDTO>()
            .ForMember(x => x.ChargePointId, z => z.MapFrom(y => y.ChargerId));
    }
}
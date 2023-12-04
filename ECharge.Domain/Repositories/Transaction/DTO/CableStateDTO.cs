using AutoMapper;
using ECharge.Domain.Entities;
using ECharge.Domain.Mappings;

namespace ECharge.Domain.Repositories.Transaction.DTO;

public class CableStateDTO : IMapFrom<CableStateHook>
{
    public int Id { get; set; }
    public string ChargePointId { get; set; }
    public string SessionId { get; set; }
    public int Connector { get; set; }
    public string CableState { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CableStateHook, CableStateDTO>();
    }
}
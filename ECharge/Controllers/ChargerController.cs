using System.Threading.Tasks;
using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.DTOs.Responses;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.EVtrip.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECharge.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ChargerController : Controller
{
    private readonly IChargePointApiClient _chargePointApi;

    public ChargerController(IChargePointApiClient chargePointApi)
    {
        _chargePointApi = chargePointApi;
    }
    
    [HttpGet]
    public async Task<ActionResult<IQueryable<ChargePointShortView>>> GetAllChargePoints()
    {
        var allChargePointsAsync = await _chargePointApi.GetAllChargePointsAsync();
        
        return Ok(allChargePointsAsync);
    }

    [HttpGet]
    public async Task<ActionResult<OperationResult<ChargePoint>>> GetSingleChargePoint(string id)
    {
      var singleChargePoint = await _chargePointApi.GetSingleChargerAsync(id);

      return Ok(singleChargePoint);
    }

    [HttpGet]
    public async Task<ActionResult<IQueryable<ChargingSession>>> GetChargingSessionsAsync(string id)
    {
        var sessions = await _chargePointApi.GetChargingSessionsAsync(id);

        return Ok(sessions);
    }

    [HttpPost]
    public async Task<ActionResult<OperationResult<ChargingSession>>> StartChargePoint(string id,
        StartChargingRequest startChargingRequest)
    {
        var startedChargePoint = await _chargePointApi.StartChargingAsync(id, startChargingRequest);

        return Ok(startedChargePoint);
    }

    [HttpPost]
    public async Task<ActionResult<OperationResult<ChargingSession>>> StopChargePoint(string id, StopChargingRequest stopChargingRequest)
    {
        var stoppedChargePoint = await _chargePointApi.StopChargingAsync(id, stopChargingRequest);

        if (!stoppedChargePoint.Success)
        {
            BadRequest();
        }

        return Ok(stoppedChargePoint);
    }
}
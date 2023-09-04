using ECharge.Domain.EVtrip.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECharge.Api.Controllers
{
    [Route("Webhooks")]
    public class WebhookController : ControllerBase
    {
        [HttpPost("cable-state-changed")]
        public IActionResult HandleCableStateChanged([FromBody] CableStateChangedPayload payload)
        {
            if (payload != null)
                return Ok($"Cable state: {payload.CableState}, Chaerger ID: {payload.ChargerId}, Connector: {payload.Connector}");
            else
                return BadRequest("Xeta");
        }

        [HttpPost("order/status-changed")]
        public IActionResult HandleOrderStatusChanged([FromBody] OrderStatusChangedPayload payload)
        {
            if (payload != null)
                return Ok($"Charger ID: {payload.ChargerId}, Connector: {payload.Connector}, Finish reason: {payload.FinishReason}, Order UUID: {payload.OrderUuid}, Status: {payload.Status}");
            else
                return BadRequest("Xeta");
        }
    }
}


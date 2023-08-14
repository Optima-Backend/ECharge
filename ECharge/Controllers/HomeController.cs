//using ECharge.Domain.CibPay.Interface;
//using ECharge.Domain.CibPay.Model;
//using ECharge.Domain.CibPay.Model.CreateOrder.Command;
//using ECharge.Domain.CibPay.Model.CreateOrder.Request;
//using ECharge.Domain.CibPay.Model.RefundOrder.Command;
//using ECharge.Domain.EVtrip.Interfaces;
//using ECharge.Domain.JWT.DTOs;
//using ECharge.Domain.JWT.Interface;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;


//namespace ECharge.Api.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly IJwtService _jwtService;
//        private readonly ICibPayService _cibPayService;

//        public HomeController(IJwtService jwtService, ICibPayService cibPayService)
//        {
//            _jwtService = jwtService;
//            _cibPayService = cibPayService;
//        }

//        //[Route("Index2")]
//        //[HttpGet]
//        //public IActionResult Index2()
//        //{
//        //    return Ok(_jwtService.GenerateJwtToken(new GenerateTokenModel { Name = "Orkhan", Surname = "Amirli", UserId = "279", Role = "Orkhan" }));
//        //}

//        //[Route("Index")]
//        //[HttpGet]
//        //[Authorize(Roles = "Heyder, Orkhan")]
//        //public IActionResult Index()
//        //{
//        //    return Ok("Success");
//        //}

//        [Route("get-ping-response")]
//        [HttpGet]
//        public async Task<IActionResult> GetPingResponse()
//        {
//            return Ok(await _cibPayService.GetPingResponse());
//        }

//        [Route("create-order")]
//        [HttpPost]
//        public async Task<IActionResult> CreateOrder(CreateOrderCommand command)
//        {
//            return Ok(await _cibPayService.CreateOrder(command));
//        }

//        [Route("get-orders")]
//        [HttpGet]
//        public async Task<IActionResult> GetOrders(GetOrdersQuery query)
//        {
//            return Ok(await _cibPayService.GetOrdersList(query));
//        }

//        [Route("get-order-by-id")]
//        [HttpGet]
//        public async Task<IActionResult> GetOrderById(string orderId)
//        {
//            return Ok(await _cibPayService.GetOrderInfo(orderId));
//        }

//        [Route("refund-order")]
//        [HttpPut]
//        public async Task<IActionResult> RefundOrder(RefundOrderCommand command)
//        {
//            return Ok(await _cibPayService.RefundOrder(command));
//        }
//    }
//}


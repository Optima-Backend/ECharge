using System.ComponentModel.DataAnnotations;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model;
using ECharge.Domain.JWT.Interface;
using ECharge.Domain.Repositories.Transaction.Interface;
using ECharge.Domain.Repositories.Transaction.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECharge.Api.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICibPayService _cibPayService;
        private readonly string _username;
        private readonly string _password;
        private readonly string _role;

        public TransactionController(IJwtService jwtService, IConfiguration configuration, ITransactionRepository transactionRepository, ICibPayService cibPayService)
        {
            _jwtService = jwtService;
            _transactionRepository = transactionRepository;
            _cibPayService = cibPayService;
            _username = configuration["Authorize:Username"];
            _password = configuration["Authorize:Password"];
            _role = configuration["Authorize:Role"];
        }

        [Route("/api/echarge/get-token")]
        [HttpGet]
        public IActionResult GetToken(string username = "ECharge", string password = "!Sm8ZKg!%sYQYTr6")
        {
            if (string.Equals(_username, username) && string.Equals(_password, password))
            {
                return Ok(new
                {
                    Token = _jwtService.GenerateJwtToken(_role)
                });
            }

            return Unauthorized(new
            {
                Message = "Incorret username or password!"
            });
        }

        [Route("/api/echarge/get-transactions")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetTransactions(TransactionQuery query)
        {
            var response = await _transactionRepository.GetAdminTransactions(query);
            return Ok(response);
        }

        [Route("/api/echarge/all-notification")]
        [HttpGet]
        public async Task<IActionResult> AllNotification(NotificationQuery query)
        {
            var response = await _transactionRepository.AllNotifications(query);
            return Ok(response);
        }

        [Route("/api/echarge/single-notification")]
        [HttpGet]
        public async Task<IActionResult> SingleNotification([Required] int id, [Required] string userId)
        {
            var response = await _transactionRepository.SingleNotification(id, userId);
            return Ok(response);
        }
        
        [Route("/api/echarge/cable-states-hooks")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetCableStates(CableStateQuery query)
        {
            var response = await _transactionRepository.GetCableStates(query);
            return Ok(response);
        }
        
        [Route("/api/echarge/order-status-changed-hooks")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetCableStates(OrderStatusChangedQuery query)
        {
            var response = await _transactionRepository.GetOrderStatusChangedHooks(query);
            return Ok(response);
        }
        
        [Route("/api/echarge/cib-orders")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetOrdersFromCib(GetOrdersQuery query)
        {
            var result = await _transactionRepository.GetAllCibTransactions(query);
            return Ok(result);
        }

        [Route("/api/echarge/transaction-report")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetTransactionsReport(GetOrdersQuery query)
        {
            var result = await _transactionRepository.GetTransactionReport(query);
            return Ok(result);
        }
        
        [Route("/api/echarge/export-transaction-excel")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetTransactionsExcel(GetOrdersQuery query)
        {
            var result = await _transactionRepository.GetTransactionExcel(query);
            
            if (result is { FileBytes.Length: > 0})
            {
                return File(result.FileBytes, result.ContentType, result.FileName);   
            }

            return Ok();
        }
        
        [Route("/api/echarge/statistics-report")]
        [HttpGet]
        [Authorize(Roles = "Authorized")]
        public async Task<IActionResult> GetStatisticsReport(GetOrdersQuery query)
        {
            var result = await _transactionRepository.GetStatisticsReport();
            return Ok(result);
        }
    }
}


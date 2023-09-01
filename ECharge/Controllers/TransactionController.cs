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
        private readonly string _username;
        private readonly string _password;
        private readonly string _role;

        public TransactionController(IJwtService jwtService, IConfiguration configuration, ITransactionRepository transactionRepository)
        {
            _jwtService = jwtService;
            _transactionRepository = transactionRepository;
            _username = configuration["Authorize:Username"];
            _password = configuration["Authorize:Password"];
            _role = configuration["Authorize:Role"];
        }

        [Route("/api/echarge/get-token")]
        [HttpGet]
        public IActionResult GetToken(string username, string password)
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminTransactions(TransactionQuery query)
        {
            try
            {
                var response = await _transactionRepository.GetAdminTransactions(query);
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}


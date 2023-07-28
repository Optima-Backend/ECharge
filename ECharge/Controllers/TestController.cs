using ECharge.Domain.JWT.DTOs;
using ECharge.Domain.JWT.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ECharge.Api.Controllers
{
    public class TestController : Controller
    {
        private readonly IJwtService _jwtService;

        public TestController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        // GET: /<controller>/
        public IActionResult GenerateToken()
        {
            var a = _jwtService.GenerateJwtToken(new GenerateTokenModel { Name = "Orkhan", Surname = "Amirli", UserId = "123", Role = "Admin" });
            return Ok(a);
        }

        // GET: /<controller>/
        public IActionResult TestToken()
        {
            var a = _jwtService.GenerateJwtToken(new GenerateTokenModel { Name = "Orkhan", Surname = "Amirli", UserId = "123", Role = "Admin" });
            return View();
        }
    }
}


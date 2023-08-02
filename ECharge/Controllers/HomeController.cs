using ECharge.Domain.JWT.DTOs;
using ECharge.Domain.JWT.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ECharge.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly IJwtService _jwtService;

        public HomeController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [Route("Index2")]
        [HttpGet]
        public IActionResult Index2()
        {
            return Ok(_jwtService.GenerateJwtToken(new GenerateTokenModel { Name = "Orkhan", Surname = "Amirli", UserId = "279", Role = "Orkhan" }));
        }

        [Route("Index")]
        [HttpGet]
        [Authorize(Roles = "Heyder, Orkhan")]
        public IActionResult Index()
        {
            return Ok("Success");
        }
    }
}


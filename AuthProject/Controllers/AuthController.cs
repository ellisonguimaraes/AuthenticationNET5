using AuthProject.Business.Interfaces;
using AuthProject.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginBusiness _loginBusiness;

        public AuthController(ILoginBusiness loginBusiness)
        {
            _loginBusiness = loginBusiness;
        }

        [HttpPost]
        [Route("signin")]
        public IActionResult Signin([FromBody] UserDTO userDTO){
            if (userDTO == null) return BadRequest("Invalid client request");

            TokenDTO token = _loginBusiness.ValidateCredentials(userDTO);

            return Ok(token);
        }

        [HttpPost]
        [Route("refresh")]
        public IActionResult Refresh([FromBody] TokenDTO tokenDTO){
            if (tokenDTO == null) return BadRequest("Invalid client request");

            var token = _loginBusiness.ValidateCredentials(tokenDTO);
            if(token == null) return BadRequest("Invalid client request");

            return Ok(token);
        }

        [HttpGet]
        [Route("revoke")]
        [Authorize("Bearer")]
        public IActionResult Revoke()
        {
            string email = User.Identity.Name;
            var result = _loginBusiness.RevokeToken(email);

            if (!result) return BadRequest("Invalid client request");

            return NoContent();
        }
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using System.Security.Claims;
using WebApi.Dto;


namespace WebApi.Controllers
{
    [Route("api/[controller]/")]
    public class AccountController : Controller
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public AccountController(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {

            var user = _mapper.Map<User>(userDto);

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {

                return BadRequest();
            }

            return Ok(); ;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user != null &&
                await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));
                return Ok("Logged in");
            }

            return BadRequest();

        }
    }
}

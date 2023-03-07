using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Service.Dtos;
using Service.Interfaces;
using System.Security.Claims;
using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]/")]
    public class AccountController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager, IEmailService emailService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var userExists = await _userManager.FindByEmailAsync(userDto.Email);

            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });

            var user = _mapper.Map<User>(userDto);

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var values = new { token, email = user.Email };
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", values, Request.Scheme);
            var message = new EmailDto { To = user.Email, Subject = "Confirmation email link", Body = confirmationLink };
            _emailService.SendEmail(message);

            return Ok(); ;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(LoginModel loginDto, string returnUrl = null)
        {

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user != null)
            {
                if(!user.EmailConfirmed)
                    return BadRequest("Invalid login attempt. You must have a confirmed email account.");

                var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));

                    return Ok("Logged in");
                }

                if (result.IsLockedOut)
                {
                    var forgotPasswordLink = this.Url.Action(nameof(ResetPassoword), "Account", new { }, Request.Scheme);
                    var content = string.Format("Your account is locked out, to reset your password, please click this link: {0}", forgotPasswordLink);

                    var message = new EmailDto { To = loginDto.Email, Subject = "Locked out account information", Body = content };
                    _emailService.SendEmail(message);

                    return BadRequest("This account is locked out.");

                }
                //else if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(LoginTwoStep), new { loginDto.Email, loginDto.RememberMe, returnUrl });
                //}
                else
                {
                    return BadRequest("Invalid login attempt. Invalid email or password.");
                }
            }

            return BadRequest("Invalid login attempt. Invalid email or password.");

        }

        //[HttpPost("LoginTwoStep")]
        //public async Task<IActionResult> LoginTwoStep()
        //{

        //}

        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return Ok("Email verified.");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPassword)
        {
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);

            if (user == null)
                return BadRequest();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var values = new { token, email = user.Email };
            var callBack = this.Url.Action(nameof(ResetPassoword), "Account", values, Request.Scheme);

            var message = new EmailDto { To = user.Email, Subject = "Reset Password Token", Body = callBack };
            _emailService.SendEmail(message);

            return Ok("Forgot password email was sent");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassoword(ResetPasswordModel resetpassword)
        {
            var user = await _userManager.FindByEmailAsync(resetpassword.Email);

            if (user == null)
                return BadRequest("User not found");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetpassword.Token, resetpassword.Password);

            if (!resetPassResult.Succeeded)
                return Ok("Your password was reset.");

            return BadRequest("It was not possible to reset your password.");
        }
    }
}

using AuthInAsp.Dto;
using AuthInAsp.IdentityService.Abstract;
using AuthInAsp.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationInCleanArchitecture.Api.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly Iuser _user;
        private readonly IConfiguration _configuration;
        public AuthController( IUserService userService, Iuser user, IConfiguration configuration)
        {
            _userService = userService;
            _user = user;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("getMe")]
        public async Task<ActionResult<string>> GetMe()
        {
            var responce = _userService.GetMyName();
            return responce;
        }

        [Authorize]
        [HttpGet("getUser")]
        public async Task<ActionResult<string>> GetUser()
        {
            string userid = _userService.GetUserId();
            int user_id = int.Parse(userid);

            var target = await _user.Get(user_id);

            if (target is null)
                return NotFound();

            return Ok(target);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User_En>> Register(Register_Dto request)
        {

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User_En user = new User_En()
            {
                Email = request.Email,
                Username = request.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await _user.Add(user);
            await _user.SaveAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(Login_Dto request)
        {
            var User = await _user.GetUserByEmail(request.Email);

            if (User == null)
            {
                return NotFound("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, User.PasswordHash, User.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(User);


            return Ok(token);
        }

        private string CreateToken(User_En user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username) ,
        new Claim(ClaimTypes.Email , user.Email),
        new Claim(ClaimTypes.SerialNumber , user.Id.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);



            return jwt;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}

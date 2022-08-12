using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.Username))
            {
                return BadRequest("Username already exist! Please choose something else.");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userToken = new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

            return userToken;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = _context.Users.SingleOrDefault(x => x.UserName == loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Invalid Username!");
            }

            var hmac = new HMACSHA512(user.PasswordSalt);

            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password!");
                }
            }

            var userToken = new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

            return userToken;
        }

        private async Task<bool> UserExist(string username)
        {
            return (await _context.Users.AnyAsync(x => x.UserName == username.ToLower()));
        }
    }
}

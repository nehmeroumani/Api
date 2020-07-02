using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using API.Helpers;
using Core;
using Core.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public interface IAccountService
    {
        AccountView Authenticate(string username, string password);
    }

    public class AccountView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public int NextAnnotationId { get; set; }
    }
    public class AccountService : IAccountService
    {

        private readonly AppSettings _appSettings;

        public AccountService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public AccountView Authenticate(string username, string password)
        {
            var user = Pool.I.Users.GetByUsername(username);


            // return null if user not found
            if (user == null)
                return null;

            if (!user.VerifyPassword(password))
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return new AccountView
            {
                Id=user.Id,
                Name = user.Name,
                Username = user.Username,
                Token = user.Token,
                Role = user.RoleName.ToString().ToLower()
            };
        }
    }
}
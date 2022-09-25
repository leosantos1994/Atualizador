using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Updater.Models;

namespace Updater.Helper
{
    public static class TokenService
    {
        public static ClaimsPrincipal GenerateClaim(User user)
        {
            var key = Encoding.ASCII.GetBytes("fedaf7d8863b48e197b9287d492b708e");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddHours(2).ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string finalToken = tokenHandler.WriteToken(token);

            var userClaims = new List<Claim>()
            {
               //define o cookie
               new Claim(ClaimTypes.Name, user.Username),
               new Claim(ClaimTypes.Hash, finalToken),
               new Claim(ClaimTypes.Role, user.Role),
               new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddHours(2).ToString()),
            };
            var minhaIdentity = new ClaimsIdentity(userClaims, "Usuario");
            return new ClaimsPrincipal(new[] { minhaIdentity });
        }

        public static string ReadToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken secToken = tokenHandler.ReadJwtToken(token);
            return secToken.Claims.FirstOrDefault(x => ClaimTypes.Sid.EndsWith(x.Type)).Value;

        }
    }
}

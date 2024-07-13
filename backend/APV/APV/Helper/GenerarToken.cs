using APV.DTOs.VeterinarioDto;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APV.Helper
{
    public class GenerarToken
    {
        private readonly IConfiguration configuration;

        public GenerarToken(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public TokenAutenticacion ConstruirToken(VeterinarioAutenticarDTO autenticarDTO, int userId)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("UserId", userId.ToString()), 
                new Claim("Email", autenticarDTO.email)
            };

            var Token = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT"]));
            var credenciales = new SigningCredentials(Token, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, 
                expires: expiracion, signingCredentials: credenciales);

            return new TokenAutenticacion() 
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };          
            
        } 
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using WebDbFirst.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebDbFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private JwtSettings _jwtSettings;
        public LoginController(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        // POST api/<LoginController>
        [HttpPost]
        public IActionResult Post([FromBody] Credentials credentials)
        {
            if (credentials != null)
            {
                string role = "User";
                // Lo username e la password, esistono nel mio db degli utenti?
                // se esiste, farò determinate azioni
                var token = GenerateJwtToken(credentials, role);
                return Ok(new { token });
                // altrimenti
                // esco subito e mando l'errore/codice di utente sconosciuto
            }
            else
            {
                // Ritengo opportuno tracciare tutti i tentativi falliti di 
                // login al sistema
                Console.WriteLine("Login fallito, credenziali nulle (OKKIO!)");
                return BadRequest("Credenziali non valide");
            }
        }
        private string GenerateJwtToken(Credentials credentials, string role)
        {
            var secretKey = _jwtSettings.SecretKey;
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(secretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                new Claim(ClaimTypes.Name, credentials.Username!),
                new Claim(ClaimTypes.Role,role),
                ]
                ),

                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
                string role = "Admin";
                try
                {
                    //int x = 1;
                    //int y = x / 0;

                    var token = GenerateJwtToken(credentials, role);

                    Response.Cookies.Append("flashMessage", credentials.ToString(), new CookieOptions
                    {
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        MaxAge = TimeSpan.FromMinutes(5)
                    });

                    var json = JsonSerializer.Serialize(credentials);
                    var b64url = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(json));

                    Response.Cookies.Append("flashCredentials", b64url, new CookieOptions
                    {
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        MaxAge = TimeSpan.FromMinutes(5)
                    });

                    Log.Information("Utente {Username} ha effettuato il login con successo.", credentials.Username);
                    return Ok(new { token });
                    // altrimenti
                    // esco subito e mando l'errore/codice di utente sconosciuto
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Errore durante il login per l'utente {Username}.", credentials.Username);
                    NLog.LogManager.GetCurrentClassLogger().Error(ex, " ############################### Errore durante il login per l'utente {0}.", credentials.Username);
                    return null;
                }
                // Lo username e la password, esistono nel mio db degli utenti?
                // se esiste, farò determinate azioni

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
                new Claim("NickName", "Claoff"),
                new Claim("Department", "IT"),
                new Claim("CustomerId", "12345"),
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

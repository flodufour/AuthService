using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthService.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class WellKnownController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WellKnownController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("jwks.json")]
        public IActionResult Jwks()
        {
            var publicKeyPem = _config["Jwt:PublicKey"]!.Replace("\\n", "\n");

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            var key = new RsaSecurityKey(rsa.ExportParameters(false))
            {
                KeyId = "authservice-key-1"
            };

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
            jwk.Use = "sig";
            jwk.Alg = SecurityAlgorithms.RsaSha256;

            return Ok(new { keys = new[] { jwk } });
        }
    }
}

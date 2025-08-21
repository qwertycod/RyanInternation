using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Homework
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;
     //   private readonly string keyVaultUrl = "https://kv260725del.vault.azure.net/";
        //private readonly string secretName  = "Issuer"; // Replace with your actual secret name


        public JwtHelper(IConfiguration config)
        {
            _config = config;
        }

        // now being accessed via Azure
        private async Task<string[]> GetIssuer(string secretAudienceName, string secretIssuerName)
        {
            var x = _config.GetConnectionString("HomeworkAudience");
            //var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            var tenantId = _config["KVT"]; // 
            var clientId = _config["KVC"]; // App (client) ID
            var clientSecret = _config["Jwt:AzAppRegKey"];          // From Certificates & Secrets
            var keyVaultUrl = "https://kv260725del.vault.azure.net/";

            // Retrieve the secret
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var client = new SecretClient(new Uri(keyVaultUrl), credential);

            Task<Response<KeyVaultSecret>> task1 = client.GetSecretAsync(secretAudienceName); // success
            Task<Response<KeyVaultSecret>> task2 = client.GetSecretAsync(secretIssuerName);

            Task[] tasks = new[] { task1, task2 };

            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
                Console.WriteLine("Some tasks failed:");

                foreach (var t in tasks)
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"- {t.Exception.InnerException.Message}");
                    }
                }
            }


            KeyVaultSecret secretAud = task1.Result.Value;
            KeyVaultSecret secretIssuer = task2.Result.Value;

            //KeyVaultSecret secretAud = await client.GetSecretAsync(secretAudienceName);
            //KeyVaultSecret secretIssuer = await client.GetSecretAsync(secretIssuerName);
            Console.WriteLine(secretAud.Value);
            Console.WriteLine(secretIssuer.Value);

            var sarray = new string[] { secretAud.Value, secretIssuer.Value };
            return sarray;
        }

        public string GenerateToken(string userName)
        {
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Not used now as the config is being loaded in program file using extension

                //var issuers = Task.Run(() => GetIssuer(_config["Jwt:IssuerKey"], _config["Jwt:AudienceKey"])).GetAwaiter().GetResult(); ;

                var issuer = _config[_config["Jwt:IssuerKey"]];
                var audience = _config[_config["Jwt:AudienceKey"]];
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience, // issuers[0], // _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: creds
                 );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public ClaimsPrincipal? ValidateRefreshToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["SecretKey"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config[_config["Jwt:IssuerKey"]],
                ValidAudience = _config[_config["Jwt:AudienceKey"]],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

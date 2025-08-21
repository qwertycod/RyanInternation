using Azure.Identity;

namespace Homework
{
    public static class ConfigurationExtensions
    {
        //method to fetch config keys from keyvault instead of getting from appsetting.json
        public static IConfigurationBuilder AddCustomConfiguration(this IConfigurationBuilder config, IWebHostEnvironment env)
        {
            var builtConfig = config.Build();

            var tenantId = builtConfig["KVT"]; 
            var clientId = builtConfig["KVC"]; // App Registration (client ID)
            var clientSecret = builtConfig["Jwt:AzAppRegKey"]; // from appsettings.json or env var

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var keyVaultUrl = builtConfig["KeyVaultUrl"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                config.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
                //  config.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
            }

            return config;
        }
    }
}

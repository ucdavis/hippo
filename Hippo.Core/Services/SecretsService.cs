using System;
using Hippo.Core.Models.Settings;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Hippo.Core.Services
{
    public interface ISecretsService
    {
        Task SetSecret(string name, string value);
        Task<string> GetSecret(string name);
        Task DeleteSecret(string name);
    }
    
    public class SecretsService: ISecretsService
    {
        private readonly AzureSettings _azureSettings;
        private readonly KeyVaultClient _vault;

        public SecretsService(IOptions<AzureSettings> azureSettings)
        {
            _azureSettings = azureSettings.Value;
            _vault = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                var credential = new ClientCredential(_azureSettings.ClientId, _azureSettings.ClientSecret);
                var token = await authContext.AcquireTokenAsync(resource, credential);

                return token.AccessToken;

            });
        }

        public async Task SetSecret(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(nameof(value));
            }

            await _vault.SetSecretAsync(_azureSettings.KeyVaultUrl, name, value);
        }
        
        public async Task<string> GetSecret(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            var secret = await _vault.GetSecretAsync(_azureSettings.KeyVaultUrl, name);
            return secret.Value;
        }

        public async Task DeleteSecret(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            await _vault.DeleteSecretAsync(_azureSettings.KeyVaultUrl, name);
        }
    }
}

using System;
using Hippo.Core.Models.Settings;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

namespace Hippo.Core.Services
{
    public interface ISecretsService
    {
        Task SetSecret(string name, string value);
        Task<string> GetSecret(string name);
        Task DeleteSecret(string name);
        Task<List<string>> GetSecretNames();
    }

    public class SecretsService : ISecretsService
    {
        private readonly SecretClient _vault;

        public SecretsService(IOptions<AzureSettings> azureSettings)
            : this(new SecretClient(new Uri(azureSettings.Value.KeyVaultUrl),
                new ClientSecretCredential(azureSettings.Value.TenantId,
                    azureSettings.Value.ClientId, azureSettings.Value.ClientSecret)))
        {
        }

        public SecretsService(SecretClient vault)
        {
            _vault = vault;
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

            await _vault.SetSecretAsync(name, value);
        }

        public async Task<string> GetSecret(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            var secret = await _vault.GetSecretAsync(name);
            return secret.Value.Value;
        }

        public async Task DeleteSecret(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            await _vault.StartDeleteSecretAsync(name);
        }

        public async Task<List<string>> GetSecretNames()
        {
            var ids = new List<string>();
            await foreach (var secret in _vault.GetPropertiesOfSecretsAsync())
            {
                ids.Add(secret.Name);
            }
            return ids;
        }
    }
}

using System;
using System.Net;
using System.Threading.Tasks;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Test
{
    [Trait("Category", "Integration")]
    public class KeyVaultSmokeTests
    {
        private const string RunSmokeTestsEnvironmentVariable = "RUN_KEYVAULT_SMOKE_TESTS";

        private readonly ITestOutputHelper _output;

        public KeyVaultSmokeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task SecretsService_CanWriteReadAndDeleteAKeyVaultSecret()
        {
            if (!string.Equals(
                    Environment.GetEnvironmentVariable(RunSmokeTestsEnvironmentVariable),
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                _output.WriteLine(
                    $"Skipping Key Vault smoke test. Set {RunSmokeTestsEnvironmentVariable}=true to run it.");
                return;
            }

            var azureSettings = GetAzureSettingsFromHippoWebUserSecrets();
            var secretsService = new SecretsService(Options.Create(azureSettings));
            var secretName = $"hippo-smoke-test-{Guid.NewGuid():N}";
            var secretValue = $"smoke-test-value-{Guid.NewGuid():N}";

            try
            {
                await secretsService.SetSecret(secretName, secretValue);

                var retrievedSecret = await secretsService.GetSecret(secretName);

                retrievedSecret.ShouldBe(secretValue);
            }
            finally
            {
                try
                {
                    await secretsService.DeleteSecret(secretName);
                }
                catch (KeyVaultErrorException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
                {
                }
            }
        }

        private static AzureSettings GetAzureSettingsFromHippoWebUserSecrets()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<KeyVaultSmokeTests>()
                .Build();

            var azureSettings = configuration.GetSection("Azure").Get<AzureSettings>();

            azureSettings.ShouldNotBeNull("The Azure section must exist in the Hippo.Web user-secrets file.");
            azureSettings.ClientId.ShouldNotBeNullOrWhiteSpace("Azure:ClientId must be set.");
            azureSettings.ClientSecret.ShouldNotBeNullOrWhiteSpace("Azure:ClientSecret must be set.");
            azureSettings.KeyVaultUrl.ShouldNotBeNullOrWhiteSpace("Azure:KeyVaultUrl must be set.");

            return azureSettings;
        }
    }
}

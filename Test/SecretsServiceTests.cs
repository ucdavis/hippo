using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Hippo.Core.Services;
using Shouldly;
using Xunit;

namespace Test;

public class SecretsServiceTests
{
    [Fact]
    public async Task PreservesValuesNamesAndPagination()
    {
        var client = new FakeSecrets();
        var service = new SecretsService(client);
        await service.SetSecret("ssh-key", "private-key-content");
        (await service.GetSecret("ssh-key")).ShouldBe("private-key-content");
        (await service.GetSecretNames()).ShouldBe(new[] { "first-page", "second-page" });
        await service.DeleteSecret("ssh-key");
        client.Deleted.ShouldBe("ssh-key");
        client.Stored.ShouldBe(("ssh-key", "private-key-content"));
    }

    [Fact]
    public async Task MissingSecretsKeepStatusForCallers()
    {
        var service = new SecretsService(new FakeSecrets());
        var exception = await Should.ThrowAsync<RequestFailedException>(() => service.GetSecret("missing"));
        exception.Status.ShouldBe(404);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task RejectsEmptyNamesAndValues(string? value)
    {
        var service = new SecretsService(new FakeSecrets());
        await Should.ThrowAsync<ArgumentException>(() => service.GetSecret(value!));
        await Should.ThrowAsync<ArgumentException>(() => service.DeleteSecret(value!));
        await Should.ThrowAsync<ArgumentException>(() => service.SetSecret(value!, "content"));
        await Should.ThrowAsync<ArgumentException>(() => service.SetSecret("name", value!));
    }

    private sealed class FakeSecrets : SecretClient
    {
        public string? Deleted { get; private set; }
        public override Task<DeleteSecretOperation> StartDeleteSecretAsync(string name, CancellationToken cancellationToken = default)
        { Deleted = name; return Task.FromResult<DeleteSecretOperation>(null!); }
        public (string, string) Stored { get; private set; }
        public override Task<Response<KeyVaultSecret>> SetSecretAsync(string name, string value, CancellationToken cancellationToken = default)
        {
            Stored = (name, value);
            return Task.FromResult(Response.FromValue(new KeyVaultSecret(name, value), (Response)null!));
        }
        public override Task<Response<KeyVaultSecret>> GetSecretAsync(string name, string version = null!, SecretContentType? outContentType = null, CancellationToken cancellationToken = default)
        {
            if (name == "missing") throw new RequestFailedException(404, "not found");
            name.ShouldBe(Stored.Item1);
            return Task.FromResult(Response.FromValue(new KeyVaultSecret(name, Stored.Item2), (Response)null!));
        }
        public override AsyncPageable<SecretProperties> GetPropertiesOfSecretsAsync(CancellationToken cancellationToken = default)
            => AsyncPageable<SecretProperties>.FromPages(new[]
            {
                Page<SecretProperties>.FromValues(new[] { new SecretProperties("first-page") }, "next", null!),
                Page<SecretProperties>.FromValues(new[] { new SecretProperties("second-page") }, null, null!)
            });
    }
}

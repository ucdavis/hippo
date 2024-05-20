
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using Hippo.Core.Models.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using YamlDotNet.Serialization;

namespace Hippo.Core.Services
{
    public interface IPuppetService
    {
        Task<PuppetData> GetPuppetData(string clusterName, string domain);
    }

    public class PuppetService : IPuppetService
    {
        private readonly PuppetSettings _settings;
        private readonly IMemoryCache _memoryCache;
        private readonly ISerializer _yamlDotNetJsonSerializer;
        public PuppetService(IOptions<PuppetSettings> settings, IMemoryCache memoryCache)
        {
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _yamlDotNetJsonSerializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
        }

        private async Task<GitHubClient> GetGithubClient()
        {
            // caching token for 10 minutes, which is the limit for GitHub API's
            var appInstallationToken = await _memoryCache.GetOrCreateAsync("github-app-installation-token", async entry =>
            {
                // create an app token to authenticate our request for an app installation token
                var now = DateTime.UtcNow;
                var handler = new JwtSecurityTokenHandler();
                var rsa = RSA.Create();
                rsa.ImportFromPem(_settings.GithubAppKey.DecodeBase64());
                var appToken = handler.WriteToken(handler.CreateJwtSecurityToken(
                    issuer: _settings.GithubAppId,
                    issuedAt: now,
                    expires: now.AddMinutes(10),
                    signingCredentials: new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
                ));
                // send request for an app installation token
                var tempGitHubClient = new GitHubClient(new ProductHeaderValue("Hippo"))
                {
                    Credentials = new Credentials(appToken, AuthenticationType.Bearer)
                };
                var accessToken = await tempGitHubClient.GitHubApps.CreateInstallationToken(_settings.GithubAppInstallationId);
                entry.SetAbsoluteExpiration(now.AddMinutes(10));
                return accessToken.Token;
            });

            var gitHubClient = new GitHubClient(new ProductHeaderValue("Hippo"))
            {
                Credentials = new Credentials(appInstallationToken, AuthenticationType.Bearer)
            };

            return gitHubClient;
        }

        public async Task<PuppetData> GetPuppetData(string clusterName, string domain)
        {
            var gitHubClient = await GetGithubClient();
            var yamlPath = $"domains/{domain}/merged/all.yaml";

            var contents = await gitHubClient.Repository.Content.GetAllContentsByRef(_settings.RepositoryOwner, _settings.RepositoryName, yamlPath, _settings.RepositoryBranch);
            var yaml = contents.First().Content;
            using var reader = new StringReader(yaml);
            var yamlDeserializer = new DeserializerBuilder()
                .WithAttemptingUnquotedStringTypeDeserialization()
                .Build();
            var rootNode = yamlDeserializer.Deserialize(reader) as Dictionary<object, object>;
            var data = new PuppetData();

            // Normalize group sponsor lists to be consistent with group member lists
            var groupSponsors = new ConcurrentDictionary<string, List<string>>();
            var groupListNode = rootNode.GetNode("group") as Dictionary<object, object>;
            foreach (var kvp in groupListNode)
            {
                var groupName = kvp.Key.ToString();
                var groupNode = kvp.Value as Dictionary<object, object>;
                var sponsors = groupNode.GetStrings("sponsors");
                // only add groups that have sponsors
                if (sponsors.Length > 0)
                {
                    data.GroupsWithSponsors.Add(new PuppetGroup 
                    {
                        Name = groupName, 
                        Data = JsonSerializer.Deserialize<JsonElement>(_yamlDotNetJsonSerializer.Serialize(groupNode))
                    });
                    foreach (var user in sponsors)
                    {
                        groupSponsors.AddOrUpdate(user, new List<string> { groupName }, (k, v) => { v.Add(groupName); return v; });
                    }
                }
            }

            var userListNode = rootNode.GetNode("user") as Dictionary<object, object>;
            var groupsSet = new HashSet<string>(data.GroupsWithSponsors.Select(g => g.Name));

            foreach (var kvp in userListNode)
            {
                var puppetUser = new PuppetUser { Kerberos = kvp.Key.ToString() };
                var userNode = kvp.Value as Dictionary<object, object>;

                puppetUser.Name = userNode.GetString("fullname");
                puppetUser.Email = userNode.GetString("email");
                // only add groups that have sponsors
                puppetUser.Groups = userNode.GetStrings("groups")
                    .Where(g => groupsSet.Contains(g))
                    .ToArray();
                if (groupSponsors.TryGetValue(puppetUser.Kerberos, out var groups))
                {
                    puppetUser.SponsorForGroups = groups.ToArray();
                }

                // remove fields we don't want stored in Data json property
                userNode.Remove("password");
                userNode.Remove("fullname");
                userNode.Remove("email");
                puppetUser.Data = JsonSerializer.Deserialize<JsonElement>(_yamlDotNetJsonSerializer.Serialize(userNode));

                data.Users.Add(puppetUser);
            }

            return data;
        }
    }

    internal static class UntypedDataExtensions
    {
        public static object GetNode(this Dictionary<object, object> node, string key)
        {
            if (node.ContainsKey(key))
            {
                return node[key];
            }
            return null;
        }

        public static string GetString(this Dictionary<object, object> node, string key)
        {
            return GetNode(node, key)?.ToString();
        }

        public static string[] GetStrings(this Dictionary<object, object> node, string key)
        {
            var sequenceNdode = GetNode(node, key) as List<object>;
            if (sequenceNdode != null)
            {
                return sequenceNdode.Select(g => g.ToString()).ToArray();
            }
            return new string[] { };
        }
    }

    public class PuppetData
    {
        public List<PuppetGroup> GroupsWithSponsors { get; set; } = new();
        public List<PuppetUser> Users { get; set; } = new();
    }

    public class PuppetUser
    {
        public string Kerberos { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string[] Groups { get; set; } = new string[] { };
        public string[] SponsorForGroups { get; set; } = new string[] { };
        public JsonElement Data { get; set; }
    }

    public class PuppetGroup
    {
        public string Name { get; set; }
        public JsonElement Data { get; set; }

    }

}
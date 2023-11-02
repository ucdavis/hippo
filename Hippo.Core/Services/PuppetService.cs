
using System.Collections.Concurrent;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Models.Settings;
using Microsoft.Extensions.Options;
using Octokit;
using YamlDotNet.RepresentationModel;

namespace Hippo.Core.Services
{
    public interface IPuppetService
    {
        Task<PuppetData> GetPuppetData(string clusterName, string domain);
    }

    public class PuppetService : IPuppetService
    {
        private readonly PuppetSettings _settings;
        private readonly GitHubClient _gitHubClient;

        public PuppetService(IOptions<PuppetSettings> settings)
        {
            _settings = settings.Value;
            _gitHubClient = new GitHubClient(new ProductHeaderValue("Hippo"))
            {
                Credentials = new Credentials(_settings.AuthToken)
            };
        }

        public async Task<PuppetData> GetPuppetData(string clusterName, string domain)
        {
            var yamlPath = $"domains/{domain}/merged/all.yaml";

            // TODO: Octokit doesn't provide a way to get at the file stream, so look into using a plain RestClient.
            var contents = await _gitHubClient.Repository.Content.GetAllContentsByRef(_settings.RepositoryOwner, _settings.RepositoryName, yamlPath, "cheeto-group-sponsors");
            var yaml = contents.First().Content;
            using var reader = new StringReader(yaml);

            // TODO: YamlStream is a bit of a misnomer. It actually loads entire root node into a DOM. Look into using the lower-level YamlParser.
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            var rootNode = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            var data = new PuppetData();

            // Normalize group sponsor lists to be consistent with group member lists
            var groupSponsors = new ConcurrentDictionary<string, List<string>>();
            var groupListNode = rootNode.GetNode("group") as YamlMappingNode;
            foreach (var kvp in groupListNode)
            {
                var groupName = kvp.Key.ToString();
                var groupNode = kvp.Value as YamlMappingNode;
                var sponsors = groupNode.GetStrings("sponsors");
                // only add groups that have sponsors
                if (sponsors.Length > 0)
                {
                    data.GroupsWithSponsors.Add(groupName);
                    foreach (var user in groupNode.GetStrings("sponsors"))
                    {
                        groupSponsors.AddOrUpdate(user, new List<string> { groupName }, (k, v) => { v.Add(groupName); return v; });
                    }
                }
            }

            var userListNode = rootNode.GetNode("user") as YamlMappingNode;
            var groupsSet = new HashSet<string>(data.GroupsWithSponsors);

            foreach (var kvp in userListNode)
            {
                var puppetUser = new PuppetUser { Kerberos = kvp.Key.ToString() };
                var userNode = kvp.Value as YamlMappingNode;

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

                data.Users.Add(puppetUser);
            }

            return data;
        }
    }

    internal static class YamlNodeExtensions
    {
        private static readonly ConcurrentDictionary<string, YamlNode> _cache = new();

        public static YamlNode GetNode(this YamlMappingNode node, string key)
        {
            var yamlKey = _cache.GetOrAdd(key, k => new YamlScalarNode(k));
            if (node.Children.ContainsKey(yamlKey))
            {
                return node.Children[yamlKey];
            }
            return null;
        }

        public static string GetString(this YamlMappingNode node, string key)
        {
            return GetNode(node, key)?.ToString();
        }

        public static string[] GetStrings(this YamlMappingNode node, string key)
        {
            var sequenceNdode = GetNode(node, key) as YamlSequenceNode;
            if (sequenceNdode != null)
            {
                return sequenceNdode.Select(g => g.ToString()).ToArray();
            }
            return new string[] { };
        }
    }

    public class PuppetData
    {
        public List<string> GroupsWithSponsors { get; set; } = new();
        public List<PuppetUser> Users { get; set; } = new();
    }

    public class PuppetUser
    {
        public string Kerberos { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string[] Groups { get; set; } = new string[] { };
        public string[] SponsorForGroups { get; set; } = new string[] { };
    }

}
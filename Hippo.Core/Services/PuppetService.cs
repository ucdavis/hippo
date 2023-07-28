
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
        Task<IEnumerable<PuppetGroup>> GetPuppetGroups(string domain);
    }
    
    public class PuppetService : IPuppetService
    {
        private readonly PuppetSettings _settings;
        private readonly GitHubClient _gitHubClient;

        public PuppetService(IOptions<PuppetSettings> settings)
        {
            _settings = settings.Value;
            _gitHubClient = new GitHubClient(new ProductHeaderValue("Hippo"));
            _gitHubClient.Credentials = new Credentials(_settings.AuthToken);
        }

        public async Task<IEnumerable<PuppetGroup>> GetPuppetGroups(string domain)
        {
            var groups = new Dictionary<string, PuppetGroup>();

            var yamlPath = $"domains/{domain}/merged/all.yaml";

            // TODO: Octokit doesn't provide a way to get at the file stream, so look into using a plain RestClient.
            var contents = await _gitHubClient.Repository.Content.GetAllContents(_settings.RepositoryOwner, _settings.RepositoryName, yamlPath);
            var yaml = contents.First().Content;
            using var reader = new StringReader(yaml);

            // TODO: YamlStream is a bit of a misnomer. It actually loads entire root node into a DOM. Look into using the lower-level YamlParser.
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            var rootNode = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            var groupKey = new YamlScalarNode("group");
            if (rootNode.Children.ContainsKey(groupKey))
            {
                var groupNode = (YamlMappingNode)rootNode.Children[groupKey];
                foreach (var kvp in groupNode)
                {
                    groups.TryAdd(kvp.Key.ToString(), new PuppetGroup { Name = kvp.Key.ToString() });
                }
            }

            var userKey = new YamlScalarNode("user");
            if (rootNode.Children.ContainsKey(userKey))
            {
                var userGroupsKey = new YamlScalarNode("groups");
                var users = (YamlMappingNode)rootNode.Children[userKey];
                foreach (var kvp in users)
                {
                    var user = new PuppetUser { Kerberos = kvp.Key.ToString() };
                    if (kvp.Value is YamlMappingNode userNode && userNode.Children.ContainsKey(userGroupsKey))
                    {
                        var userGroups = (YamlSequenceNode)userNode.Children[userGroupsKey];
                        foreach (var groupNode in userGroups)
                        {
                            if (groups.TryGetValue(groupNode.ToString(), out var group))
                            {
                                user.Groups.Add(group);
                                group.Users.Add(user);
                            }
                        }
                    }
                }
            }

            return groups.Values;
        }
    }
}
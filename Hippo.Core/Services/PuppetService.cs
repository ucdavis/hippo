
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
        Task<IEnumerable<PuppetGroupPuppetUser>> GetPuppetGroupsUsers(string clusterName, string domain);
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

        public async Task<IEnumerable<PuppetGroupPuppetUser>> GetPuppetGroupsUsers(string clusterName, string domain)
        {
            var yamlPath = $"domains/{domain}/merged/all.yaml";

            // TODO: Octokit doesn't provide a way to get at the file stream, so look into using a plain RestClient.
            var contents = await _gitHubClient.Repository.Content.GetAllContents(_settings.RepositoryOwner, _settings.RepositoryName, yamlPath);
            var yaml = contents.First().Content;
            using var reader = new StringReader(yaml);

            // TODO: YamlStream is a bit of a misnomer. It actually loads entire root node into a DOM. Look into using the lower-level YamlParser.
            var yamlStream = new YamlStream();
            yamlStream.Load(reader);
            var rootNode = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            var results = new List<PuppetGroupPuppetUser>();
            var userKey = new YamlScalarNode("user");
            if (rootNode.Children.ContainsKey(userKey))
            {
                var userGroupsKey = new YamlScalarNode("groups");
                var users = (YamlMappingNode)rootNode.Children[userKey];
                foreach (var kvp in users)
                {
                    var kerberos = kvp.Key.ToString();
                    if (kvp.Value is YamlMappingNode userNode && userNode.Children.ContainsKey(userGroupsKey))
                    {
                        var userGroups = (YamlSequenceNode)userNode.Children[userGroupsKey];
                        foreach (var groupNode in userGroups)
                        {
                            results.Add(new PuppetGroupPuppetUser { ClusterName = clusterName, GroupName = groupNode.ToString(), UserKerberos = kerberos });
                        }
                    }
                }
            }

            return results;
        }
    }
}
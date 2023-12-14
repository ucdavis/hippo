
namespace Hippo.Core.Models.Settings
{
    public class PuppetSettings
    {
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
        public string RepositoryBranch { get; set; } = "main";
        public string GithubAppKey { get; set; }
        public string GithubAppId { get; set; }
        public long GithubAppInstallationId { get; set; }
    }
}

namespace Hippo.Core.Models.Settings
{
    public class PuppetSettings
    {
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
        public string RepositoryBranch { get; set; } = "main";
        public string AuthToken { get; set; }
    }
}
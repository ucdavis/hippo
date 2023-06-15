using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models.Settings;
using Ietws;
using Microsoft.Extensions.Options;

namespace Hippo.Core.Services
{
    public interface IIdentityService
    {
        Task<User> GetByEmail(string email);
        Task<User> GetByKerberos(string kerb);
    }

    public class IdentityService : IIdentityService
    {
        private readonly AppDbContext _dbContext;
        private readonly AuthSettings _authSettings;

        public IdentityService(AppDbContext dbContext, IOptions<AuthSettings> authSettings)
        {
            _dbContext = dbContext;
            _authSettings = authSettings.Value;
        }

        public async Task<User> GetByEmail(string email)
        {
            var clientws = new IetClient(_authSettings.IamKey);
            // get IAM from email
            var iamResult = await clientws.Contacts.Search(ContactSearchField.email, email);
            var iamId = iamResult.ResponseData.Results.Length > 0 ? iamResult.ResponseData.Results[0].IamId : string.Empty;
            if (string.IsNullOrWhiteSpace(iamId))
            {
                return null;
            }
            // return info for the user identified by this IAM 
            var result = await clientws.Kerberos.Search(KerberosSearchField.iamId, iamId);

            if (result.ResponseData.Results.Length > 0)
            {
                var ucdKerbPerson = result.ResponseData.Results.First();
                var user = CreateUser(email, ucdKerbPerson, iamId);
                return user;
            }
            return null;
        }

        public async Task<User> GetByKerberos(string kerb)
        {
            var clientws = new IetClient(_authSettings.IamKey);
            var ucdKerbResult = await clientws.Kerberos.Search(KerberosSearchField.userId, kerb);

            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return null;
            }

            if (ucdKerbResult.ResponseData.Results.Length != 1)
            {
                var iamIds = ucdKerbResult.ResponseData.Results.Select(a => a.IamId).Distinct().ToArray();
                var userIDs = ucdKerbResult.ResponseData.Results.Select(a => a.UserId).Distinct().ToArray();
                if (iamIds.Length != 1 && userIDs.Length != 1)
                {
                    throw new Exception($"IAM issue with non unique values for kerbs: {string.Join(',', userIDs)} IAM: {string.Join(',', iamIds)}");
                }
            }

            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.First();

            // find their email
            var ucdContactResult = await clientws.Contacts.Get(ucdKerbPerson.IamId);

            if (ucdContactResult.ResponseData.Results.Length == 0)
            {
                return null;
            }

            var ucdContact = ucdContactResult.ResponseData.Results.First();
            var rtValue = CreateUser(ucdContact.Email, ucdKerbPerson, ucdKerbPerson.IamId);

            if (string.IsNullOrWhiteSpace(rtValue.Email))
            {
                if (!string.IsNullOrWhiteSpace(ucdKerbPerson.UserId))
                {
                    rtValue.Email = $"{ucdKerbPerson.UserId}@ucdavis.edu";
                }
            }

            return rtValue;
        }

        private User CreateUser(string email, KerberosResult ucdKerbPerson, string iamId)
        {
            var user = new User()
            {
                FirstName = ucdKerbPerson.FirstName,
                LastName = ucdKerbPerson.LastName,
                Kerberos = ucdKerbPerson.UserId,
                Email = email,
                Iam = iamId,
                MothraId = ucdKerbPerson.MothraId
            };
            return user;
        }
    }
}

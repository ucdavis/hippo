
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using EFCore.BulkExtensions;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Hippo.Core.Services
{
    public interface IAccountSyncService
    {
        Task<bool> Run();
    }

    public class AccountSyncService : IAccountSyncService
    {
        private readonly IPuppetService _puppetService;
        private readonly AppDbContext _dbContext;
        private readonly IHistoryService _historyService;

        public AccountSyncService(IPuppetService puppetService, AppDbContext dbContext, IHistoryService historyService)
        {
            _puppetService = puppetService;
            _dbContext = dbContext;
            _historyService = historyService;
        }

        public async Task<bool> Run()
        {
            try
            {
                var clusters = await _dbContext.Clusters
                    .Where(c => !string.IsNullOrEmpty(c.Domain))
                    .AsNoTracking()
                    .ToArrayAsync();

                Log.Information("Syncing accounts for clusters {ClusterNames}", string.Join(", ", clusters.Select(c => c.Name)));

                var now = DateTime.UtcNow;
                var mapClusterIdsToPuppetData = new Dictionary<int, PuppetDataContext>();

                foreach (var cluster in clusters)
                {
                    var puppetData = await _puppetService.GetPuppetData(cluster.Name, cluster.Domain);
                    mapClusterIdsToPuppetData.Add(cluster.Id, new PuppetDataContext { ClusterId = cluster.Id, PuppetData = puppetData });
                }

                // refresh temp table data
                await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempGroups]");
                await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempKerberos]");
                await _dbContext.BulkInsertAsync(mapClusterIdsToPuppetData.SelectMany(x => x.Value.PuppetData.Users.Select(u => new TempKerberos { ClusterId = x.Key, Kerberos = u.Kerberos })));
                await _dbContext.BulkInsertAsync(mapClusterIdsToPuppetData.SelectMany(x => x.Value.PuppetData.GroupsWithSponsors.Select(g => new TempGroup { ClusterId = x.Key, Group = g.Name })));

                var mapKerbsToUserIds = await _dbContext.Users
                    .Where(u => _dbContext.TempKerberos.Any(tg => tg.Kerberos == u.Kerberos))
                    .Select(u => new { u.Id, u.Kerberos })
                    .ToDictionaryAsync(k => k.Kerberos, v => v.Id);

                // setup desired state of groups and accounts
                var desiredAccounts = mapClusterIdsToPuppetData
                    .SelectMany(x => x.Value.PuppetData.Users.Select(u => new Account
                    {
                        Name = u.Name,
                        Email = u.Email,
                        Kerberos = u.Kerberos,
                        OwnerId = mapKerbsToUserIds.ContainsKey(u.Kerberos) ? mapKerbsToUserIds[u.Kerberos] : null,
                        ClusterId = x.Key,
                        CreatedOn = now,
                        UpdatedOn = now,
                        Data = ExpandQosReferences(u.Data, x.Value)
                    }))
                    .Where(a => IsValid(a))
                    .ToArray();
                var desiredGroups = mapClusterIdsToPuppetData
                    .SelectMany(x => x.Value.PuppetData.GroupsWithSponsors.Select(g => new Group
                    {
                        Name = g.Name,
                        DisplayName = g.Name,
                        ClusterId = x.Key,
                        Data = ExpandQosReferences(g.Data, x.Value)
                    }))
                    .Where(g => IsValid(g))
                    .ToArray();

                // Determine what groups and accounts need to be deleted.
                // We can't use BulkInsertOrUpdateOrDeleteAsync because operations against GroupMemberAccount and GroupAdminAccount must occur
                // between inserts and deletes of groups and accounts.
                var deleteGroups = await _dbContext.Groups
                    .Where(g => !_dbContext.TempGroups.Any(tg => tg.ClusterId == g.ClusterId && tg.Group == g.Name))
                    .ToArrayAsync();
                var deleteAccounts = await _dbContext.Accounts
                    .Where(a => !_dbContext.TempKerberos.Any(tk => tk.ClusterId == a.ClusterId && tk.Kerberos == a.Kerberos))
                    .ToArrayAsync();

                // insert/update groups and accounts
                Log.Information("Inserting/Updating {GroupQuantity} groups and {AccountQuantity} accounts",
                    desiredGroups.Length, desiredAccounts.Length);
                await _dbContext.BulkInsertOrUpdateAsync(desiredGroups, new BulkConfig
                {
                    PropertiesToExcludeOnUpdate = new List<string> { nameof(Group.DisplayName) },
                    UpdateByProperties = new List<string> { nameof(Group.ClusterId), nameof(Group.Name) }
                });
                await _dbContext.BulkInsertOrUpdateAsync(desiredAccounts, new BulkConfig
                {
                    PropertiesToExcludeOnUpdate = new List<string> { nameof(Account.SshKey), nameof(Account.CreatedOn) },
                    PropertiesToExcludeOnCompare = new List<string> { nameof(Account.UpdatedOn), nameof(Account.CreatedOn) },
                    UpdateByProperties = new List<string> { nameof(Account.ClusterId), nameof(Account.Kerberos) },
                    BatchSize = 500
                });

                // get ids for groups and accounts
                // NOTE: this must occur after the accounts have been inserted/updated to guarantee presence of ids
                var mapClusterIdAndGroupNameToId = await _dbContext.Groups
                    .ToDictionaryAsync(g => (g.ClusterId, g.Name), g => g.Id);

                var mapClusterIdAndKerberosToAccountId = await _dbContext.Accounts
                    .Where(a => a.Kerberos != null)
                    .ToDictionaryAsync(a => (a.ClusterId, a.Kerberos), a => a.Id);

                // setup desired state of group memberships
                var desiredGroupAccounts = mapClusterIdsToPuppetData
                    .SelectMany(x => x.Value.PuppetData.Users
                        .Where(u => mapClusterIdAndKerberosToAccountId.ContainsKey((x.Key, u.Kerberos)))
                        .SelectMany(u => u.Groups.Select(g => new GroupMemberAccount
                        {
                            GroupId = mapClusterIdAndGroupNameToId[(x.Key, g)],
                            AccountId = mapClusterIdAndKerberosToAccountId[(x.Key, u.Kerberos)]
                        })));

                var desiredGroupAdminAccounts = mapClusterIdsToPuppetData
                    .SelectMany(x => x.Value.PuppetData.Users
                        .Where(u => mapClusterIdAndKerberosToAccountId.ContainsKey((x.Key, u.Kerberos)))
                        .SelectMany(u => u.SponsorForGroups.Select(g => new GroupAdminAccount
                        {
                            GroupId = mapClusterIdAndGroupNameToId[(x.Key, g)],
                            AccountId = mapClusterIdAndKerberosToAccountId[(x.Key, u.Kerberos)]
                        })));

                // insert/update/delete group memberships
                Log.Information("Inserting/Updating/Deleting {GroupMembershipQuantity} group memberships and {GroupAdminMembershipQuantity} group admin memberships",
                    desiredGroupAccounts.Count(), desiredGroupAdminAccounts.Count());
                await _dbContext.BulkInsertOrUpdateOrDeleteAsync(desiredGroupAccounts);
                await _dbContext.BulkInsertOrUpdateOrDeleteAsync(desiredGroupAdminAccounts);

                // delete groups and accounts
                Log.Information("Deleting {GroupQuantity} groups and {AccountQuantity} accounts", deleteGroups.Length, deleteAccounts.Length);
                await _dbContext.BulkDeleteAsync(deleteGroups, new BulkConfig
                {
                    UpdateByProperties = new List<string> { nameof(Group.ClusterId), nameof(Group.Name) }
                });
                await _dbContext.BulkDeleteAsync(deleteAccounts, new BulkConfig
                {
                    UpdateByProperties = new List<string> { nameof(Account.ClusterId), nameof(Account.Kerberos) }
                });

                // clear temp table data
                await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempGroups]");
                await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TempKerberos]");

                // Update any requests that are now complete
                var sqlQuery = $@"
                    UPDATE r
                    SET r.Status = '{Request.Statuses.Completed}', r.UpdatedOn = @UpdatedOn
                    FROM Requests r
                    INNER JOIN Accounts a ON r.ClusterId = a.ClusterId
                    INNER JOIN Users u ON r.RequesterId = u.Id AND u.Kerberos = a.Kerberos
                    WHERE r.Status = 'Processing' AND EXISTS (
                        SELECT 1 FROM Groups g
                        INNER JOIN GroupMemberAccount gma ON g.Id = gma.GroupId
                        WHERE g.Name = r.[Group] AND gma.AccountId = a.Id)";
                int requestsCompleted = await _dbContext.Database.ExecuteSqlRawAsync(sqlQuery,
                    new SqlParameter("@UpdatedOn", now));
                Log.Information("Marked {RequestsCompleted} requests 'Completed'", requestsCompleted);

                await _historyService.PuppetDataSynced();
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error syncing accounts");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Converts <paramref name="data"/> to a <seealso cref="JsonElement"/> with all string qos references replaced
        /// with the objects they reference.
        /// </summary>
        /// <remarks>
        /// A qos string of 'gpum-users-gpum-qos' would map to the object found under $.group.gpum-users.slurm.partitions.gpum.qos
        /// </remarks>
        private JsonElement ExpandQosReferences(JsonNode data, PuppetDataContext context)
        {
            var partitions = data["slurm"]?["partitions"]?.AsObject();
            if (partitions != null)
            {
                foreach (var kvp in partitions.Where(kvp => kvp.Value != null))
                {
                    var stringQosNode = kvp.Value?["qos"];
                    if (stringQosNode is not JsonValue qosStringValue)
                        continue; // no value property named "qos"
                    if (!qosStringValue.TryGetValue<string>(out var qosString))
                        continue; // qos property is not a string
                    var match = System.Text.RegularExpressions.Regex.Match(qosString, "(?'groupAndPartition'.+?)-qos");
                    if (!match.Success)
                        continue; // qos string doesn't what we were expecting
                    var expandedQosNode = context.MapQosStringToQosObject.GetOrAdd(qosString, key =>
                    {
                        // With an unfortunate convention of separating group and partition names with a character that can appear
                        // unescaped in both group and partition names, we're forced to iterate through every possible combination
                        // and check to see if it exists in the dom...
                        foreach ((var groupName, var partitionName) in GetPossibleGroupAndPartitionNames(match.Groups["groupAndPartition"].Value))
                        {
                            var group = context.PuppetData.Groups.FirstOrDefault(g => g.Name == groupName);
                            if (group == null)
                                continue; // no groups found matching groupName
                            var objectQosNode = group.Data?["slurm"]?["partitions"]?[partitionName]?["qos"];
                            if (objectQosNode == null)
                                continue; // no qos found for given partitionName
                            return objectQosNode;
                        }
                        // fallback to original value...
                        return stringQosNode;
                    });
                    if (expandedQosNode is not JsonObject)
                        continue;
                    // TODO: replace Deserialize with DeepClone; available in dotnet 8
                    kvp.Value["qos"] = expandedQosNode.Deserialize<JsonObject>();
                }
            }
            return data.Deserialize<JsonElement>();
        }

        static IEnumerable<(string, string)> GetPossibleGroupAndPartitionNames(string groupAndPartition)
        {
            var segments = groupAndPartition.Split("-");
            if (segments.Length < 2)
                throw new ArgumentException($"{nameof(groupAndPartition)} should contain at least two segments");
            for (var i = 1; i < segments.Length; i++)
            {
                yield return (string.Join("-", segments.Take(i)), string.Join("-", segments.Skip(i)));
            }
        }

        static bool IsValid<T>(T obj)
        {
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(obj, new ValidationContext(obj), results, true))
            {
                Log.Warning("Invalid {Type}: {Errors}", typeof(T).Name, results.Select(r => r.ErrorMessage));
                return false;
            }
            return true;
        }

        private class PuppetDataContext
        {
            public int ClusterId { get; set; }
            public PuppetData PuppetData { get; set; }
            public ConcurrentDictionary<string, JsonNode> MapQosStringToQosObject { get; set; } = new();
        }
    }
}
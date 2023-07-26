// using Hippo.Core.Data;
// using Hippo.Core.Domain;
// using Hippo.Core.Models;
// using Hippo.Core.Services;
// using Hippo.Web.Extensions;
// using Hippo.Web.Models;
// using Hippo.Web.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Serilog;
// using static Hippo.Core.Domain.Account;

// namespace Hippo.Web.Controllers;

// [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
// public class AdminController : SuperController
// {
//     private AppDbContext _dbContext;
//     private IUserService _userService;
//     private IIdentityService _identityService;
//     private IHistoryService _historyService;
//     private ISshService _sshService;
//     private INotificationService _notificationService;

//     public AdminController(AppDbContext dbContext, IUserService userService, IIdentityService identityService, ISshService sshService, INotificationService notificationService, IHistoryService historyService)
//     {
//         _dbContext = dbContext;
//         _userService = userService;
//         _identityService = identityService;
//         _historyService = historyService;
//         _sshService = sshService;
//         _notificationService = notificationService;
//     }

//     [HttpGet]
//     public async Task<IActionResult> Index()
//     {
//         var currentUser = await _userService.GetCurrentUser();

//         return Ok(await _dbContext.Accounts
//             .CanAccess(_dbContext, Cluster, currentUser.Iam)
//             .Include(a => a.Owner)
//             .AsNoTracking()
//             .OrderBy(a => a.Owner.FirstName)
//             .ThenBy(a => a.Owner.LastName)
//             .ToListAsync());
//     }

//     [HttpPost]
//     public async Task<IActionResult> Create(string id)
//     {
//         if (string.IsNullOrWhiteSpace(id))
//         {
//             return BadRequest("You must supply either an email or kerb id to lookup.");
//         }

//         var userLookup = id.Contains("@")
//                     ? await _identityService.GetByEmail(id)
//                     : await _identityService.GetByKerberos(id);
//         if (userLookup == null)
//         {
//             return BadRequest("User Not Found");
//         }

//         // try to find user account in the cluster
//         var clusterAccount = await _dbContext.Accounts
//             .Include(a => a.Owner)
//             .Include(a => a.Cluster)
//             .InCluster(Cluster)
//             .Where(a => a.Owner.Iam == userLookup.Iam)
//             .SingleOrDefaultAsync();

//         if (clusterAccount != null)
//         {
//             // user found with existing account.  upgrade to admin
//             if (clusterAccount.IsAdmin)
//             {
//                 return BadRequest("User is already an admin.");
//             }
//             clusterAccount.IsAdmin = true;
//         }
//         else
//         {
//             // need to create new account, but user might already exist
//             var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == userLookup.Iam);

//             if (user == null)
//             {
//                 // user doesn't exist, assing them to found user
//                 user = userLookup;
//             }

//             // now we have the user but need to create the account
//             clusterAccount = new Account
//             {
//                 Name = user.Name,
//                 CanSponsor = false,
//                 Owner = user,
//                 IsAdmin = true,
//                 IsActive = true,
//                 Status = Account.Statuses.Active,
//                 Cluster = await _dbContext.Clusters.SingleAsync(c => c.Name == Cluster)
//             };

//             _dbContext.Accounts.Add(clusterAccount);
//         }

//         await _historyService.AddHistory("Admin role added", $"Kerb: {clusterAccount.Owner.Kerberos} IAM: {clusterAccount.Owner.Iam} Email: {clusterAccount.Owner.Email} Name: {clusterAccount.Owner.Name}", clusterAccount);

//         await _dbContext.SaveChangesAsync();
//         return Ok(clusterAccount);
//     }

//     [HttpPost]
//     public async Task<IActionResult> Remove(int id)
//     {
//         // TODO: change from userID to account ID

//         // try to find user account in the cluster
//         var clusterAccount = await _dbContext.Accounts.Include(a => a.Owner).Include(a => a.Cluster).InCluster(Cluster).Where(a => a.Owner.Id == id).SingleOrDefaultAsync();

//         if (clusterAccount == null)
//         {
//             return NotFound();
//         }

//         if (clusterAccount.Owner.Id == (await _userService.GetCurrentUser()).Id)
//         {
//             return BadRequest("Can't remove yourself");
//         }

//         clusterAccount.IsAdmin = false;

//         await _historyService.AddHistory("Admin role removed", $"Kerb: {clusterAccount.Owner.Kerberos} IAM: {clusterAccount.Owner.Iam} Email: {clusterAccount.Owner.Email} Name: {clusterAccount.Owner.Name}", clusterAccount);

//         await _dbContext.SaveChangesAsync();
//         return Ok();
//     }



//     [HttpGet]
//     public async Task<IActionResult> Sponsors()
//     {
//         return Ok(await _dbContext.Accounts.Include(a => a.Owner).InCluster(Cluster).Where(a => a.CanSponsor).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
//     }

//     [HttpPost]
//     public async Task<IActionResult> CreateSponsor([FromBody] SponsorCreateModel model)
//     {
//         if (string.IsNullOrWhiteSpace(model.Lookup))
//         {
//             return BadRequest("You must supply either an email or kerb id to lookup.");
//         }



//         var userLookup = model.Lookup.Contains("@")
//                     ? await _identityService.GetByEmail(model.Lookup)
//                     : await _identityService.GetByKerberos(model.Lookup);
//         if (userLookup == null)
//         {
//             return BadRequest("User Not Found");
//         }

//         var user = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == userLookup.Iam);
//         if (user == null)
//         {
//             user = userLookup;
//             await _dbContext.Users.AddAsync(user);
//         }

//         var isNewAccount = false;

//         var account = await _dbContext.Accounts.Include(a => a.Cluster).InCluster(Cluster).SingleOrDefaultAsync(a => a.OwnerId == user.Id);
//         if (account != null)
//         {
//             if (account.Status != Statuses.Active)
//             {
//                 return BadRequest($"Existing Account for user is not in the Active status: {account.Status}");
//             }
//             var saveCanSponsor = account.CanSponsor;
//             var saveName = account.Name;
//             account.CanSponsor = true;
//             account.SponsorId = null; //They are being made so clear out the sponsor id on their account.
//             if (!string.IsNullOrWhiteSpace(model.Name))
//             {
//                 account.Name = model.Name;
//                 await _historyService.AddAccountHistory(account, "NameUpdated");
//                 await _historyService.AddHistory("Sponsor name updated", $"Old Name: {saveName} New Name: {account.Name}", account);
//             }
//             if (!saveCanSponsor)
//             {
//                 await _historyService.AddAccountHistory(account, "MadeSponsor");
//                 await _historyService.AddHistory("Sponsor role added", $"New Account: {isNewAccount}", account);
//             }
//         }
//         else
//         {
//             var cluster = await _dbContext.Clusters.SingleAsync(a => a.Name == Cluster);
//             account = new Account
//             {
//                 Status = Statuses.Active,
//                 Name = string.IsNullOrWhiteSpace(model.Name) ? user.Name : model.Name,
//                 Owner = user,
//                 CanSponsor = true,
//                 Cluster = cluster,
//             };

//             await _dbContext.Accounts.AddAsync(account);
//             await _historyService.AddAccountHistory(account, "CreatedSponsor");

//             isNewAccount = true;
//             await _historyService.AddHistory("Sponsor role added", $"New Account: {isNewAccount} Name Used: {account.Name}", account);
//         }



//         await _dbContext.SaveChangesAsync();

//         return StatusCode(isNewAccount ? StatusCodes.Status201Created : StatusCodes.Status200OK, account);
//     }

//     [HttpPost]
//     public async Task<IActionResult> RemoveSponsor(int id)
//     {
//         var account = await _dbContext.Accounts.Include(a => a.Cluster).InCluster(Cluster).SingleOrDefaultAsync(a => a.Id == id);
//         if (account == null)
//         {
//             return NotFound();
//         }


//         account.CanSponsor = false;
//         await _historyService.AddAccountHistory(account, "RemovedSponsor");
//         await _historyService.AddHistory("Sponsor role removed", null, account);
//         await _dbContext.SaveChangesAsync();
//         return Ok();
//     }

//     [HttpPost]
//     public async Task<IActionResult> ChangeSponsorOwner(int id, [FromBody] SponsorCreateModel model)
//     {
//         var clusterId = await _dbContext.Clusters.Where(a => a.Name == Cluster).Select(c => c.Id).SingleAsync();
//         var originalSponsorAccount = await _dbContext.Accounts.Include(a => a.Owner).InCluster(Cluster).SingleAsync(a => a.Id == id);

//         if (string.IsNullOrWhiteSpace(model.Lookup))
//         {
//             return BadRequest("You must supply either an email or kerb id to lookup.");
//         }

//         var userLookup = model.Lookup.Contains("@")
//                     ? await _identityService.GetByEmail(model.Lookup)
//                     : await _identityService.GetByKerberos(model.Lookup);
//         if (userLookup == null)
//         {
//             return BadRequest("User Not Found");
//         }

//         using (var txn = await _dbContext.Database.BeginTransactionAsync())
//         {
//             // get the user represented by lookup values and create if not found 
//             var newOwner = await _dbContext.Users.SingleOrDefaultAsync(a => a.Iam == userLookup.Iam);
//             if (newOwner == null)
//             {
//                 newOwner = userLookup;
//                 await _dbContext.Users.AddAsync(newOwner);
//             }

//             // new we need to ensure this user has an account with a sponsor role
//             var newSponsorAccount = await _dbContext.Accounts.InCluster(Cluster).SingleOrDefaultAsync(a => a.Owner.Iam == userLookup.Iam);
//             if (newSponsorAccount == null)
//             {
//                 // no account in ths cluster, create one
//                 newSponsorAccount = new Account
//                 {
//                     Status = Statuses.Active,
//                     Name = string.IsNullOrWhiteSpace(model.Name) ? newOwner.Name : model.Name,
//                     Owner = newOwner,
//                     CanSponsor = true,
//                     ClusterId = clusterId,
//                 };

//                 await _dbContext.Accounts.AddAsync(newSponsorAccount);
//                 await _historyService.AddAccountHistory(newSponsorAccount, "MadeSponsor");
//             }
//             else
//             {
//                 newSponsorAccount.Name = string.IsNullOrWhiteSpace(model.Name) ? newSponsorAccount.Name : model.Name;
//                 newSponsorAccount.SponsorId = null;
//                 // they already have an account.  if they can't sponsor yet then add that role
//                 if (!newSponsorAccount.CanSponsor)
//                 {
//                     newSponsorAccount.CanSponsor = true;
//                     newSponsorAccount.Status = Statuses.Active; //Force to active if there is a pending account
//                     await _historyService.AddAccountHistory(newSponsorAccount, "MadeSponsor");
//                 }


//             }

//             // save our changes to ensure we have user and account Ids
//             await _dbContext.SaveChangesAsync();

//             // now we have a user and account for the new sponsor
//             // need to find everyone who was sponsored by old sponsor and transfer to new sponsor
//             var accountsSponsoredByOriginal = await _dbContext.Accounts.InCluster(Cluster).Where(a => a.SponsorId == originalSponsorAccount.Id).ToListAsync();

//             foreach (var acct in accountsSponsoredByOriginal)
//             {
//                 acct.SponsorId = newSponsorAccount.Id;
//                 await _historyService.AddAccountHistory(acct, "SponsorChanged");
//             }

//             // now let's mark our old sponsor as no longer a sponsor
//             originalSponsorAccount.CanSponsor = false;
//             await _historyService.AddAccountHistory(originalSponsorAccount, "RemovedSponsor");

//             await _historyService.AddHistory("Sponsor Account Transfered", $"Sponsored accounts transfered from owner: {originalSponsorAccount.Owner.Kerberos} IAM: {originalSponsorAccount.Owner.Iam} Email: {originalSponsorAccount.Owner.Email} Name: {originalSponsorAccount.Owner.Name}", originalSponsorAccount);
//             await _historyService.AddHistory("Sponsor Account Transfered", $"Sponsored accounts transfered to owner: {newSponsorAccount.Owner.Kerberos} IAM: {newSponsorAccount.Owner.Iam} Email: {newSponsorAccount.Owner.Email} Name: {newSponsorAccount.Owner.Name}", newSponsorAccount);

//             await _dbContext.SaveChangesAsync();

//             await txn.CommitAsync();

//             return StatusCode(StatusCodes.Status200OK, newSponsorAccount);
//         }
//     }

//     // Return all accounts that are waiting for any sponsor to approve for cluster
//     [HttpGet]
//     public async Task<ActionResult> Pending()
//     {
//         return Ok(await _dbContext.Accounts.InCluster(Cluster).Where(a => a.Status == Account.Statuses.PendingApproval).Include(a => a.Sponsor).ThenInclude(a => a.Owner).AsNoTracking().OrderBy(a => a.Name).ToListAsync());
//     }

//     // Approve a given pending account 
//     [HttpPost]
//     public async Task<ActionResult> Approve(int id)
//     {
//         var currentUser = await _userService.GetCurrentUser();

//         var account = await _dbContext.Accounts.Include(a => a.Owner).Include(a => a.Cluster).InCluster(Cluster).AsSingleQuery()
//             .SingleOrDefaultAsync(a => a.Id == id && a.Status == Account.Statuses.PendingApproval);

//         if (account == null)
//         {
//             return NotFound();
//         }

//         var connectionInfo = await _dbContext.Clusters.GetSshConnectionInfo(Cluster);

//         var tempFileName = $"/var/lib/remote-api/.{account.Owner.Kerberos}.yaml"; //Leading .
//         var fileName = $"/var/lib/remote-api/{account.Owner.Kerberos}.yaml";

//         await _sshService.PlaceFile(account.SshKey, tempFileName, connectionInfo);
//         await _sshService.RenameFile(tempFileName, fileName, connectionInfo);

//         account.Status = Account.Statuses.Active;


//         var success = await _notificationService.AccountDecision(account, true, "Admin Override");
//         if (!success)
//         {
//             Log.Error("Error creating Account Decision email");
//         }

//         success = await _notificationService.AdminOverrideDecision(account, true, currentUser); //Notify sponsor
//         if (!success)
//         {
//             Log.Error("Error creating Admin Override Decision email");
//         }

//         await _historyService.AccountApproved(account);

//         await _historyService.AddHistory("Account override approve", $"Kerb: {account.Owner.Kerberos} IAM: {account.Owner.Iam} Email: {account.Owner.Email} Name: {account.Owner.Name}", account);


//         await _dbContext.SaveChangesAsync();

//         return Ok();
//     }

//     [HttpPost]
//     public async Task<ActionResult> Reject(int id, [FromBody] RequestRejectionModel model)
//     {
//         if (String.IsNullOrWhiteSpace(model.Reason))
//         {
//             return BadRequest("Missing Reject Reason");
//         }

//         var currentUser = await _userService.GetCurrentUser();

//         var account = await _dbContext.Accounts.Include(a => a.Owner).Include(a => a.Cluster).InCluster(Cluster).AsSingleQuery()
//             .SingleOrDefaultAsync(a => a.Id == id && a.Status == Account.Statuses.PendingApproval);

//         if (account == null)
//         {
//             return NotFound();
//         }


//         account.Status = Account.Statuses.Rejected;
//         account.IsActive = false;

//         var success = await _notificationService.AccountDecision(account, false, "Admin Override", reason: model.Reason);
//         if (!success)
//         {
//             Log.Error("Error creating Account Decision email");
//         }
//         success = await _notificationService.AdminOverrideDecision(account, false, currentUser, reason: model.Reason); //Notify sponsor
//         if (!success)
//         {
//             Log.Error("Error creating Admin Override Decision email");
//         }

//         await _historyService.AccountRejected(account, model.Reason);

//         await _historyService.AddHistory("Account override rejected", $"Kerb: {account.Owner.Kerberos} IAM: {account.Owner.Iam} Email: {account.Owner.Email} Name: {account.Owner.Name} Reason: {model.Reason}", account);


//         await _dbContext.SaveChangesAsync();

//         return Ok();
//     }
// }

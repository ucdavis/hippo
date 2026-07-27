using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Test.Helpers;
using Xunit;

namespace Test.Services;

public class AccountUpdateServiceTests
{
    [Fact]
    public async Task UpdateEvent_CompleteCreateAccount_LinksExistingSyncedAccount()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var user = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        var syncedAccount = new Account
        {
            Cluster = cluster,
            Kerberos = "pat",
            Name = "Pat",
            Email = "pat@example.com"
        };
        dbContext.AddRange(cluster, user, group, syncedAccount);
        await dbContext.SaveChangesAsync();

        var acceptedAupOn = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        var request = new Request
        {
            Requester = user,
            RequesterId = user.Id,
            Group = group.Name,
            Action = Request.Actions.CreateAccount,
            Status = Request.Statuses.Processing,
            Cluster = cluster,
            ClusterId = cluster.Id
        }.WithAccountRequestData(new AccountRequestDataModel
        {
            AcceptableUsePolicyAgreedOn = acceptedAupOn,
            AccessTypes = new List<string>()
        });
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var queuedEvent = new QueuedEvent
        {
            Action = QueuedEvent.Actions.CreateAccount,
            Status = QueuedEvent.Statuses.Pending,
            Data = QueuedEventDataModel.FromRequestAndGroup(request, group),
            Request = request,
            RequestId = request.Id
        };
        dbContext.QueuedEvents.Add(queuedEvent);
        await dbContext.SaveChangesAsync();

        var service = new AccountUpdateService(dbContext, new CapturingHistoryService());

        var result = await service.UpdateEvent(queuedEvent, QueuedEvent.Statuses.Complete);

        result.IsError.ShouldBeFalse();
        queuedEvent.Status.ShouldBe(QueuedEvent.Statuses.Complete);
        request.Status.ShouldBe(Request.Statuses.Completed);

        var account = await dbContext.Accounts
            .IgnoreQueryFilters()
            .Include(a => a.MemberOfGroups)
            .SingleAsync(a => a.ClusterId == cluster.Id && a.Kerberos == user.Kerberos);
        account.OwnerId.ShouldBe(user.Id);
        account.AcceptableUsePolicyAgreedOn.ShouldBe(acceptedAupOn);
        account.MemberOfGroups.Select(g => g.Id).ShouldContain(group.Id);

        var accountCount = await dbContext.Accounts
            .IgnoreQueryFilters()
            .CountAsync(a => a.ClusterId == cluster.Id && a.Kerberos == user.Kerberos);
        accountCount.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateEvent_CompleteCreateAccount_FailsWhenExistingAccountBelongsToAnotherUser()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var requester = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var existingOwner = new User
        {
            FirstName = "Existing",
            LastName = "Owner",
            Email = "owner@example.com",
            Iam = "100000002",
            Kerberos = "owner",
            MothraId = "mothra2"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        var existingAccount = new Account
        {
            Cluster = cluster,
            Kerberos = "pat",
            Name = "Pat",
            Email = "pat@example.com",
            Owner = existingOwner
        };
        dbContext.AddRange(cluster, requester, existingOwner, group, existingAccount);
        await dbContext.SaveChangesAsync();

        var request = new Request
        {
            Requester = requester,
            RequesterId = requester.Id,
            Group = group.Name,
            Action = Request.Actions.CreateAccount,
            Status = Request.Statuses.Processing,
            Cluster = cluster,
            ClusterId = cluster.Id
        }.WithAccountRequestData(new AccountRequestDataModel());
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var queuedEvent = new QueuedEvent
        {
            Action = QueuedEvent.Actions.CreateAccount,
            Status = QueuedEvent.Statuses.Pending,
            Data = QueuedEventDataModel.FromRequestAndGroup(request, group),
            Request = request,
            RequestId = request.Id
        };
        dbContext.QueuedEvents.Add(queuedEvent);
        await dbContext.SaveChangesAsync();

        var service = new AccountUpdateService(dbContext, new CapturingHistoryService());

        var result = await service.UpdateEvent(queuedEvent, QueuedEvent.Statuses.Complete);

        result.IsError.ShouldBeTrue();
        queuedEvent.Status.ShouldBe(QueuedEvent.Statuses.Failed);
        existingAccount.OwnerId.ShouldBe(existingOwner.Id);
    }

    [Fact]
    public async Task UpdateEvent_CompleteCreateAccount_FailsWhenKerberosMatchesMultipleUsersWithoutIam()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var cluster = new Cluster { Name = "farm" };
        var requester = new User
        {
            FirstName = "Pat",
            LastName = "Requester",
            Email = "pat@example.com",
            Iam = "100000001",
            Kerberos = "pat",
            MothraId = "mothra1"
        };
        var duplicateUser = new User
        {
            FirstName = "Other",
            LastName = "Requester",
            Email = "other@example.com",
            Iam = "100000002",
            Kerberos = "pat",
            MothraId = "mothra2"
        };
        var group = new Group { Cluster = cluster, Name = "research", DisplayName = "Research" };
        dbContext.AddRange(cluster, requester, duplicateUser, group);
        await dbContext.SaveChangesAsync();

        var request = new Request
        {
            Requester = requester,
            RequesterId = requester.Id,
            Group = group.Name,
            Action = Request.Actions.CreateAccount,
            Status = Request.Statuses.Processing,
            Cluster = cluster,
            ClusterId = cluster.Id
        }.WithAccountRequestData(new AccountRequestDataModel());
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var queuedEvent = new QueuedEvent
        {
            Action = QueuedEvent.Actions.CreateAccount,
            Status = QueuedEvent.Statuses.Pending,
            Data = QueuedEventDataModel.FromRequestAndGroup(request, group),
            Request = request,
            RequestId = request.Id
        };
        queuedEvent.Data.Accounts.Single().Iam = "";
        dbContext.QueuedEvents.Add(queuedEvent);
        await dbContext.SaveChangesAsync();

        var service = new AccountUpdateService(dbContext, new CapturingHistoryService());

        var result = await service.UpdateEvent(queuedEvent, QueuedEvent.Statuses.Complete);

        result.IsError.ShouldBeTrue();
        result.Message.ShouldBe("Multiple users found for Kerberos pat; cannot determine account owner");
        queuedEvent.Status.ShouldBe(QueuedEvent.Statuses.Failed);
        var accountCreated = await dbContext.Accounts
            .IgnoreQueryFilters()
            .AnyAsync(a => a.ClusterId == cluster.Id && a.Kerberos == requester.Kerberos);
        accountCreated.ShouldBeFalse();
    }

    private class CapturingHistoryService : IHistoryService
    {
        public List<History> AddedHistory { get; } = new();

        public Task AddHistory(History history, string clusterName = null!)
        {
            AddedHistory.Add(history);
            return Task.CompletedTask;
        }
    }
}

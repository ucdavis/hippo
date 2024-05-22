using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Data
{
    public sealed class AppDbContextSqlite : AppDbContext
    {
        public AppDbContextSqlite(DbContextOptions<AppDbContextSqlite> options) : base(options)
        {
        }
    }

    public sealed class AppDbContextSqlServer : AppDbContext
    {
        public AppDbContextSqlServer(DbContextOptions<AppDbContextSqlServer> options) : base(options)
        {
        }
    }

    public abstract class AppDbContext : DbContext
    {
        protected AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Cluster> Clusters { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<GroupAdminAccount> GroupAdminAccount { get; set; }
        public virtual DbSet<GroupMemberAccount> GroupMemberAccount { get; set; }
        public virtual DbSet<TempGroup> TempGroups { get; set; }
        public virtual DbSet<TempKerberos> TempKerberos { get; set; }
        public virtual DbSet<QueuedEvent> QueuedEvents { get; set; }
        public virtual DbSet<AccessType> AccessTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            Account.OnModelCreating(builder, this);
            User.OnModelCreating(builder);
            History.OnModelCreating(builder);
            Cluster.OnModelCreating(builder);
            Group.OnModelCreating(builder, this);
            Role.OnModelCreating(builder);
            Permission.OnModelCreating(builder);
            Request.OnModelCreating(builder, this);
            Domain.GroupAdminAccount.OnModelCreating(builder);
            Domain.GroupMemberAccount.OnModelCreating(builder);
            QueuedEvent.OnModelCreating(builder, this);
            AccessType.OnModelCreating(builder);
        }
    }
 }

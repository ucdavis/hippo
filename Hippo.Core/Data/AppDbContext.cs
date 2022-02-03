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
        public virtual DbSet<EmailHistory> EmailHistories { get; set; }
    }
 }

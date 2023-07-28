﻿// <auto-generated />
using System;
using Hippo.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    [DbContext(typeof(AppDbContextSqlServer))]
    [Migration("20230728191315_PuppetGroups")]
    partial class PuppetGroups
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Hippo.Core.Domain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int?>("GroupId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<int?>("SponsorId")
                        .HasColumnType("int");

                    b.Property<string>("SshKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("CreatedOn");

                    b.HasIndex("GroupId");

                    b.HasIndex("Name");

                    b.HasIndex("OwnerId");

                    b.HasIndex("SponsorId");

                    b.HasIndex("UpdatedOn");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Hippo.Core.Domain.AccountHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("ActorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Note")
                        .HasMaxLength(1500)
                        .HasColumnType("nvarchar(1500)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("ActorId");

                    b.ToTable("AccountHistories");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Cluster", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("Domain")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("SshKeyId")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("SshName")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("SshUrl")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("Clusters");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId", "Name")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("AccountId")
                        .HasColumnType("int");

                    b.Property<int>("ActedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("ActedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Action")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("AdminAction")
                        .HasColumnType("bit");

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<string>("Details")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("ActedById");

                    b.HasIndex("ClusterId");

                    b.ToTable("Histories");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("ClusterId")
                        .HasColumnType("int");

                    b.Property<int?>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("GroupId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Hippo.Core.Domain.PuppetGroup", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.HasKey("Name");

                    b.ToTable("PuppetGroups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.PuppetUser", b =>
                {
                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Kerberos");

                    b.ToTable("PuppetUsers");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Hippo.Core.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Iam")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("MothraId")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("Iam")
                        .IsUnique()
                        .HasFilter("[Iam] IS NOT NULL");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PuppetGroupPuppetUser", b =>
                {
                    b.Property<string>("GroupsName")
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("UsersKerberos")
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("GroupsName", "UsersKerberos");

                    b.HasIndex("UsersKerberos");

                    b.ToTable("PuppetGroupPuppetUser");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Account", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany("Accounts")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Group", "Group")
                        .WithMany("Accounts")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Hippo.Core.Domain.User", "Owner")
                        .WithMany("Accounts")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Account", "Sponsor")
                        .WithMany()
                        .HasForeignKey("SponsorId");

                    b.Navigation("Cluster");

                    b.Navigation("Group");

                    b.Navigation("Owner");

                    b.Navigation("Sponsor");
                });

            modelBuilder.Entity("Hippo.Core.Domain.AccountHistory", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Account", "Account")
                        .WithMany("Histories")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.User", "Actor")
                        .WithMany()
                        .HasForeignKey("ActorId");

                    b.Navigation("Account");

                    b.Navigation("Actor");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Group", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany("Groups")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cluster");
                });

            modelBuilder.Entity("Hippo.Core.Domain.History", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Hippo.Core.Domain.User", "ActedBy")
                        .WithMany()
                        .HasForeignKey("ActedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany()
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("ActedBy");

                    b.Navigation("Cluster");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Permission", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany()
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Hippo.Core.Domain.Group", "Group")
                        .WithMany("Permissions")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Hippo.Core.Domain.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.User", "User")
                        .WithMany("Permissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cluster");

                    b.Navigation("Group");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PuppetGroupPuppetUser", b =>
                {
                    b.HasOne("Hippo.Core.Domain.PuppetGroup", null)
                        .WithMany()
                        .HasForeignKey("GroupsName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.PuppetUser", null)
                        .WithMany()
                        .HasForeignKey("UsersKerberos")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Hippo.Core.Domain.Account", b =>
                {
                    b.Navigation("Histories");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Cluster", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Groups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Group", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Permissions");
                });

            modelBuilder.Entity("Hippo.Core.Domain.User", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Permissions");
                });
#pragma warning restore 612, 618
        }
    }
}

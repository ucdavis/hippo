﻿// <auto-generated />
using System;
using Hippo.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    [DbContext(typeof(AppDbContextSqlite))]
    partial class AppDbContextSqliteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.2");

            modelBuilder.Entity("Hippo.Core.Domain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int?>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SponsorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SshKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("TEXT");

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
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ActorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasMaxLength(1500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("ActorId");

                    b.ToTable("AccountHistories");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Cluster", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("Domain")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("SshKeyId")
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("SshName")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("SshUrl")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("Clusters");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId", "Name")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ActedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Action")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<bool>("AdminAction")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

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
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

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
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("PuppetGroups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.PuppetUser", b =>
                {
                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Kerberos");

                    b.ToTable("PuppetUsers");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Hippo.Core.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Iam")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("MothraId")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("Iam")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PuppetGroupPuppetUser", b =>
                {
                    b.Property<string>("GroupsName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UsersKerberos")
                        .HasColumnType("TEXT");

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

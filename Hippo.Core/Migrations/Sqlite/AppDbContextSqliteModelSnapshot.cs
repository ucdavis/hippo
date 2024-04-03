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
            modelBuilder.HasAnnotation("ProductVersion", "6.0.19");

            modelBuilder.Entity("AccessTypeAccount", b =>
                {
                    b.Property<int>("AccessTypesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccountsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AccessTypesId", "AccountsId");

                    b.HasIndex("AccountsId");

                    b.ToTable("AccessTypeAccount");
                });

            modelBuilder.Entity("AccessTypeCluster", b =>
                {
                    b.Property<int>("AccessTypesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClustersId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AccessTypesId", "ClustersId");

                    b.HasIndex("ClustersId");

                    b.ToTable("AccessTypeCluster");
                });

            modelBuilder.Entity("Hippo.Core.Domain.AccessType", b =>
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

                    b.ToTable("AccessTypes");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SshKey")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("CreatedOn");

                    b.HasIndex("Email");

                    b.HasIndex("Kerberos");

                    b.HasIndex("Name");

                    b.HasIndex("OwnerId");

                    b.HasIndex("UpdatedOn");

                    b.ToTable("Accounts");
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

                    b.Property<string>("Email")
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

            modelBuilder.Entity("Hippo.Core.Domain.GroupAdminAccount", b =>
                {
                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AccountId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupAdminAccount");
                });

            modelBuilder.Entity("Hippo.Core.Domain.GroupMemberAccount", b =>
                {
                    b.Property<int>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AccountId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupMemberAccount");
                });

            modelBuilder.Entity("Hippo.Core.Domain.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ActedById")
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

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ActedById");

                    b.HasIndex("ActedDate");

                    b.HasIndex("Action");

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

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Hippo.Core.Domain.QueuedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("TEXT");

                    b.Property<int?>("RequestId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Action");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("RequestId");

                    b.HasIndex("Status");

                    b.HasIndex("UpdatedAt");

                    b.ToTable("QueuedEvents");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ActorId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClusterId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .HasColumnType("TEXT");

                    b.Property<string>("Group")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<int>("RequesterId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SshKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("SupervisingPI")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Action");

                    b.HasIndex("ActorId");

                    b.HasIndex("ClusterId");

                    b.HasIndex("Group");

                    b.HasIndex("RequesterId");

                    b.HasIndex("Status");

                    b.ToTable("Requests");
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

            modelBuilder.Entity("Hippo.Core.Domain.TempGroup", b =>
                {
                    b.Property<string>("Group")
                        .HasColumnType("TEXT");

                    b.HasKey("Group");

                    b.ToTable("TempGroups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.TempKerberos", b =>
                {
                    b.Property<string>("Kerberos")
                        .HasColumnType("TEXT");

                    b.HasKey("Kerberos");

                    b.ToTable("TempKerberos");
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

            modelBuilder.Entity("AccessTypeAccount", b =>
                {
                    b.HasOne("Hippo.Core.Domain.AccessType", null)
                        .WithMany()
                        .HasForeignKey("AccessTypesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Account", null)
                        .WithMany()
                        .HasForeignKey("AccountsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccessTypeCluster", b =>
                {
                    b.HasOne("Hippo.Core.Domain.AccessType", null)
                        .WithMany()
                        .HasForeignKey("AccessTypesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Cluster", null)
                        .WithMany()
                        .HasForeignKey("ClustersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Hippo.Core.Domain.Account", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany("Accounts")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.User", "Owner")
                        .WithMany("Accounts")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Cluster");

                    b.Navigation("Owner");
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

            modelBuilder.Entity("Hippo.Core.Domain.GroupAdminAccount", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("Hippo.Core.Domain.GroupMemberAccount", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("Hippo.Core.Domain.History", b =>
                {
                    b.HasOne("Hippo.Core.Domain.User", "ActedBy")
                        .WithMany()
                        .HasForeignKey("ActedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany()
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ActedBy");

                    b.Navigation("Cluster");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Permission", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany()
                        .HasForeignKey("ClusterId")
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

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Hippo.Core.Domain.QueuedEvent", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Request", "Request")
                        .WithMany("QueuedEvents")
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Request");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Request", b =>
                {
                    b.HasOne("Hippo.Core.Domain.User", "Actor")
                        .WithMany()
                        .HasForeignKey("ActorId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany()
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.User", "Requester")
                        .WithMany()
                        .HasForeignKey("RequesterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Actor");

                    b.Navigation("Cluster");

                    b.Navigation("Requester");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Cluster", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Groups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Request", b =>
                {
                    b.Navigation("QueuedEvents");
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

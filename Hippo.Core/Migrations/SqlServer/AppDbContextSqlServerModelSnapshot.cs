﻿// <auto-generated />
using System;
using Hippo.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    [DbContext(typeof(AppDbContextSqlServer))]
    partial class AppDbContextSqlServerModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.19")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("AccessTypeAccount", b =>
                {
                    b.Property<int>("AccessTypesId")
                        .HasColumnType("int");

                    b.Property<int>("AccountsId")
                        .HasColumnType("int");

                    b.HasKey("AccessTypesId", "AccountsId");

                    b.HasIndex("AccountsId");

                    b.ToTable("AccessTypeAccount");
                });

            modelBuilder.Entity("AccessTypeCluster", b =>
                {
                    b.Property<int>("AccessTypesId")
                        .HasColumnType("int");

                    b.Property<int>("ClustersId")
                        .HasColumnType("int");

                    b.HasKey("AccessTypesId", "ClustersId");

                    b.HasIndex("ClustersId");

                    b.ToTable("AccessTypeCluster");
                });

            modelBuilder.Entity("Hippo.Core.Domain.AccessType", b =>
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

                    b.ToTable("AccessTypes");
                });

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

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("SshKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("datetime2");

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

            modelBuilder.Entity("Hippo.Core.Domain.Billing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ChartString")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<decimal>("Percentage")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("Billings");
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

                    b.Property<string>("Email")
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

            modelBuilder.Entity("Hippo.Core.Domain.FinancialDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<bool>("AutoApprove")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("ChartString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<string>("FinancialSystemApiSource")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("SecretAccessKey")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId")
                        .IsUnique();

                    b.ToTable("FinancialDetails");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

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

            modelBuilder.Entity("Hippo.Core.Domain.GroupAdminAccount", b =>
                {
                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.HasKey("AccountId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupAdminAccount");
                });

            modelBuilder.Entity("Hippo.Core.Domain.GroupMemberAccount", b =>
                {
                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.HasKey("AccountId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupMemberAccount");
                });

            modelBuilder.Entity("Hippo.Core.Domain.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("ActedById")
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

                    b.Property<int?>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ActedById");

                    b.HasIndex("ActedDate");

                    b.HasIndex("Action");

                    b.HasIndex("ClusterId");

                    b.HasIndex("OrderId");

                    b.ToTable("Histories");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("Adjustment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("AdjustmentReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("BalanceRemaining")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("ExternalReference")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<int>("Installments")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PrincipalInvestigatorId")
                        .HasColumnType("int");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SubTotal")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Units")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("PrincipalInvestigatorId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Hippo.Core.Domain.OrderMetaData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("OrderId", "Name", "Value");

                    b.ToTable("MetaData");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("CreatedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Details")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FinancialSystemId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrackingNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("OrderId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("ClusterId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<int>("Installments")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Units")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Hippo.Core.Domain.QueuedEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Action")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RequestId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

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
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("ActorId")
                        .HasColumnType("int");

                    b.Property<int>("ClusterId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Data")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Group")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<int>("RequesterId")
                        .HasColumnType("int");

                    b.Property<string>("SshKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("SupervisingPI")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("datetime2");

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

            modelBuilder.Entity("Hippo.Core.Domain.TempGroup", b =>
                {
                    b.Property<string>("Group")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Group");

                    b.ToTable("TempGroups");
                });

            modelBuilder.Entity("Hippo.Core.Domain.TempKerberos", b =>
                {
                    b.Property<string>("Kerberos")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Kerberos");

                    b.ToTable("TempKerberos");
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

            modelBuilder.Entity("Hippo.Core.Domain.Billing", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Order", "Order")
                        .WithMany("Billings")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Hippo.Core.Domain.FinancialDetail", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithOne("FinancialDetail")
                        .HasForeignKey("Hippo.Core.Domain.FinancialDetail", "ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cluster");
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

                    b.HasOne("Hippo.Core.Domain.Order", "Order")
                        .WithMany("History")
                        .HasForeignKey("OrderId");

                    b.Navigation("ActedBy");

                    b.Navigation("Cluster");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Order", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany("Orders")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Hippo.Core.Domain.User", "PrincipalInvestigator")
                        .WithMany("Orders")
                        .HasForeignKey("PrincipalInvestigatorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cluster");

                    b.Navigation("PrincipalInvestigator");
                });

            modelBuilder.Entity("Hippo.Core.Domain.OrderMetaData", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Order", "Order")
                        .WithMany("MetaData")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Payment", b =>
                {
                    b.HasOne("Hippo.Core.Domain.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.HasOne("Hippo.Core.Domain.Order", "Order")
                        .WithMany("Payments")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("Order");
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

            modelBuilder.Entity("Hippo.Core.Domain.Product", b =>
                {
                    b.HasOne("Hippo.Core.Domain.Cluster", "Cluster")
                        .WithMany("Products")
                        .HasForeignKey("ClusterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cluster");
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

                    b.Navigation("FinancialDetail");

                    b.Navigation("Groups");

                    b.Navigation("Orders");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Order", b =>
                {
                    b.Navigation("Billings");

                    b.Navigation("History");

                    b.Navigation("MetaData");

                    b.Navigation("Payments");
                });

            modelBuilder.Entity("Hippo.Core.Domain.Request", b =>
                {
                    b.Navigation("QueuedEvents");
                });

            modelBuilder.Entity("Hippo.Core.Domain.User", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Orders");

                    b.Navigation("Permissions");
                });
#pragma warning restore 612, 618
        }
    }
}

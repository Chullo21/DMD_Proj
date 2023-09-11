﻿// <auto-generated />
using System;
using DMD_Prototype.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DMD_Prototype.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230908031623_addemailsecdom")]
    partial class addemailsecdom
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DMD_Prototype.Models.AccountModel", b =>
                {
                    b.Property<int>("AccID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AccID"));

                    b.Property<string>("AccName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Dom")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sec")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AccID");

                    b.ToTable("AccountDb");
                });

            modelBuilder.Entity("DMD_Prototype.Models.InstructionModel", b =>
                {
                    b.Property<int>("InsID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("InsID"));

                    b.Property<byte[]>("InsPhoto")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("Page")
                        .HasColumnType("int");

                    b.HasKey("InsID");

                    b.ToTable("InsDb");
                });

            modelBuilder.Entity("DMD_Prototype.Models.MTIModel", b =>
                {
                    b.Property<int>("MTIID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MTIID"));

                    b.Property<byte[]>("Documnet1")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("Documnet2")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("Documnet3")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("Documnet4")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Numberchuchu")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MTIID");

                    b.ToTable("MTIDb");
                });
#pragma warning restore 612, 618
        }
    }
}

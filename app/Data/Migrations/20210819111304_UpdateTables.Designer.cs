﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using app.Data;

namespace app.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210819111304_UpdateTables")]
    partial class UpdateTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("app.Entities.AppUser", b =>
                {
                    b.Property<long>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("AuthDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CurrentBranch")
                        .HasColumnType("text");

                    b.Property<string>("EpumpDataId")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("State")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("ChatId");

                    b.HasIndex("EpumpDataId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("app.Entities.EpumpData", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("AuthKey")
                        .HasColumnType("text");

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<string>("CompanyId")
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("EpumpData");
                });

            modelBuilder.Entity("app.Entities.AppUser", b =>
                {
                    b.HasOne("app.Entities.EpumpData", "EpumpData")
                        .WithOne("User")
                        .HasForeignKey("app.Entities.AppUser", "EpumpDataId");

                    b.Navigation("EpumpData");
                });

            modelBuilder.Entity("app.Entities.EpumpData", b =>
                {
                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using app.Data;

namespace app.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210803123443_RemoveBranches")]
    partial class RemoveBranches
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.8");

            modelBuilder.Entity("app.Entities.AppUser", b =>
                {
                    b.Property<long>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AuthDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("EpumpDataId")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Hash")
                        .HasColumnType("TEXT");

                    b.Property<string>("State")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ChatId");

                    b.HasIndex("EpumpDataId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("app.Entities.Company", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("EpumpDataId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EpumpDataId")
                        .IsUnique();

                    b.ToTable("Company");
                });

            modelBuilder.Entity("app.Entities.EpumpData", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("TEXT");

                    b.Property<string>("AuthKey")
                        .HasColumnType("TEXT");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CompanyId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Role")
                        .HasColumnType("TEXT");

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

            modelBuilder.Entity("app.Entities.Company", b =>
                {
                    b.HasOne("app.Entities.EpumpData", "EpumpData")
                        .WithOne("Company")
                        .HasForeignKey("app.Entities.Company", "EpumpDataId");

                    b.Navigation("EpumpData");
                });

            modelBuilder.Entity("app.Entities.EpumpData", b =>
                {
                    b.Navigation("Company");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}

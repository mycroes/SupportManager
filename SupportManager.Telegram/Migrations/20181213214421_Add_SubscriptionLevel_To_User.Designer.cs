﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SupportManager.Telegram.DAL;

namespace SupportManager.Telegram.Migrations
{
    [DbContext(typeof(UserDbContext))]
    [Migration("20181213214421_Add_SubscriptionLevel_To_User")]
    partial class Add_SubscriptionLevel_To_User
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("SupportManager.Telegram.DAL.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApiKey");

                    b.Property<long>("ChatId");

                    b.Property<int?>("DefaultTeamId");

                    b.Property<int>("SubscriptionLevel");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}

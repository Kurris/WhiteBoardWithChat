﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WhiteBoard.EF;

namespace WhiteBoard.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20210412070836_addfirst")]
    partial class addfirst
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("WhiteBoard.Entity.Online", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasMaxLength(14);

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasMaxLength(14);

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<long>("RoomCode")
                        .HasColumnType("bigint");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Onlines");
                });

            modelBuilder.Entity("WhiteBoard.Entity.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasMaxLength(14);

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("IsNeedPassword")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasMaxLength(14);

                    b.Property<int>("Owner")
                        .HasColumnType("int");

                    b.Property<string>("Password")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<long>("RoomCode")
                        .HasColumnType("bigint");

                    b.Property<string>("RoomName")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("Rooms");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Room");
                });

            modelBuilder.Entity("WhiteBoard.Entity.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime(6)")
                        .HasMaxLength(14);

                    b.Property<DateTime?>("ModifyTime")
                        .HasColumnType("datetime(6)")
                        .HasMaxLength(14);

                    b.Property<string>("Password")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("RealName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("SignalRConnectionId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("WhiteBoard.Model.RoomDTO", b =>
                {
                    b.HasBaseType("WhiteBoard.Entity.Room");

                    b.Property<string>("Moderator")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.ToTable("Rooms");

                    b.HasDiscriminator().HasValue("RoomDTO");
                });
#pragma warning restore 612, 618
        }
    }
}
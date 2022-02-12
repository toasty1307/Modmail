﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Modmail.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modmail.Data.Migrations
{
    [DbContext(typeof(GuildContext))]
    [Migration("20220211162217_nullable")]
    partial class nullable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Modmail.Data.Entities.ConfigEntity", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("CategoryChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LogAccessRoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LogChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("ModThreadOpenMessage")
                        .HasColumnType("text");

                    b.Property<bool>("MoveChannelsToCategory")
                        .HasColumnType("boolean");

                    b.Property<string>("UserThreadOpenMessage")
                        .HasColumnType("text");

                    b.HasKey("GuildId");

                    b.ToTable("Configs");
                });

            modelBuilder.Entity("Modmail.Data.Entities.FileEntity", b =>
                {
                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<decimal?>("MessageEntityMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Url");

                    b.HasIndex("MessageEntityMessageId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Modmail.Data.Entities.GuildEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("IconUrl")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Setup")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("IconUrl");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Modmail.Data.Entities.MessageEntity", b =>
                {
                    b.Property<decimal>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("Anonymous")
                        .HasColumnType("boolean");

                    b.Property<string>("AuthorAvatarUrl")
                        .HasColumnType("text");

                    b.Property<string>("AuthorDiscriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("AuthorId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("AuthorUsername")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("ThreadId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("MessageId");

                    b.HasIndex("AuthorAvatarUrl");

                    b.HasIndex("ThreadId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Modmail.Data.Entities.ThreadEntity", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("Open")
                        .HasColumnType("boolean");

                    b.Property<string>("RecipientAvatarUrl")
                        .HasColumnType("text");

                    b.Property<decimal>("RecipientId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("RecipientName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.HasIndex("RecipientAvatarUrl");

                    b.ToTable("Threads");
                });

            modelBuilder.Entity("Modmail.Data.Entities.ConfigEntity", b =>
                {
                    b.HasOne("Modmail.Data.Entities.GuildEntity", "GuildEntity")
                        .WithOne("Config")
                        .HasForeignKey("Modmail.Data.Entities.ConfigEntity", "GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildEntity");
                });

            modelBuilder.Entity("Modmail.Data.Entities.FileEntity", b =>
                {
                    b.HasOne("Modmail.Data.Entities.MessageEntity", null)
                        .WithMany("Attachments")
                        .HasForeignKey("MessageEntityMessageId");
                });

            modelBuilder.Entity("Modmail.Data.Entities.GuildEntity", b =>
                {
                    b.HasOne("Modmail.Data.Entities.FileEntity", "Icon")
                        .WithMany()
                        .HasForeignKey("IconUrl");

                    b.Navigation("Icon");
                });

            modelBuilder.Entity("Modmail.Data.Entities.MessageEntity", b =>
                {
                    b.HasOne("Modmail.Data.Entities.FileEntity", "AuthorAvatar")
                        .WithMany()
                        .HasForeignKey("AuthorAvatarUrl");

                    b.HasOne("Modmail.Data.Entities.ThreadEntity", "ThreadEntity")
                        .WithMany("Messages")
                        .HasForeignKey("ThreadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuthorAvatar");

                    b.Navigation("ThreadEntity");
                });

            modelBuilder.Entity("Modmail.Data.Entities.ThreadEntity", b =>
                {
                    b.HasOne("Modmail.Data.Entities.GuildEntity", "GuildEntity")
                        .WithMany("Threads")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Modmail.Data.Entities.FileEntity", "RecipientAvatar")
                        .WithMany()
                        .HasForeignKey("RecipientAvatarUrl");

                    b.Navigation("GuildEntity");

                    b.Navigation("RecipientAvatar");
                });

            modelBuilder.Entity("Modmail.Data.Entities.GuildEntity", b =>
                {
                    b.Navigation("Config")
                        .IsRequired();

                    b.Navigation("Threads");
                });

            modelBuilder.Entity("Modmail.Data.Entities.MessageEntity", b =>
                {
                    b.Navigation("Attachments");
                });

            modelBuilder.Entity("Modmail.Data.Entities.ThreadEntity", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}

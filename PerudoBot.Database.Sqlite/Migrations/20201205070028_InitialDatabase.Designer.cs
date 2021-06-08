﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PerudoBot.Database.Data;

namespace PerudoBot.Database.Sqlite.Migrations
{
    [DbContext(typeof(PerudoBotDbContext))]
    [Migration("20201205070028_InitialDatabase")]
    partial class InitialDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Action", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ActionType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double?>("DurationInSeconds")
                        .HasColumnType("REAL");

                    b.Property<int>("GamePlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GamePlayerRoundId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAutoAction")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsOutOfTurn")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ParentActionId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GamePlayerId");

                    b.HasIndex("GamePlayerRoundId");

                    b.HasIndex("ParentActionId");

                    b.HasIndex("RoundId");

                    b.ToTable("Actions");

                    b.HasDiscriminator<string>("ActionType").HasValue("Action");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CanCallExactToJoinAgain")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateFinished")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateStarted")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("PlayerTurnId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundStartPlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("StatusMessage")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Winner")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.GamePlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Dice")
                        .HasColumnType("TEXT");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NumberOfDice")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TurnOrder")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("PlayerId");

                    b.ToTable("GamePlayers");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.GamePlayerRound", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Dice")
                        .HasColumnType("TEXT");

                    b.Property<int>("GamePlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NumberOfDice")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TurnOrder")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GamePlayerId");

                    b.HasIndex("RoundId");

                    b.ToTable("GamePlayerRounds");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBot")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nickname")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Round", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateFinished")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateStarted")
                        .HasColumnType("TEXT");

                    b.Property<double?>("DurationInSeconds")
                        .HasColumnType("REAL");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RoundType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StartingPlayerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Rounds");

                    b.HasDiscriminator<string>("RoundType").HasValue("Round");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Bid", b =>
                {
                    b.HasBaseType("PerudoBot.Database.Sqlite.Data.Action");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Pips")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Quantity")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("Bid");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.ExactCall", b =>
                {
                    b.HasBaseType("PerudoBot.Database.Sqlite.Data.Action");

                    b.HasDiscriminator().HasValue("ExactCall");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.LiarCall", b =>
                {
                    b.HasBaseType("PerudoBot.Database.Sqlite.Data.Action");

                    b.HasDiscriminator().HasValue("LiarCall");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.FaceoffRound", b =>
                {
                    b.HasBaseType("PerudoBot.Database.Sqlite.Data.Round");

                    b.HasDiscriminator().HasValue("FaceoffRound");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.PalificoRound", b =>
                {
                    b.HasBaseType("PerudoBot.Database.Sqlite.Data.Round");

                    b.HasDiscriminator().HasValue("PalificoRound");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.StandardRound", b =>
                {
                    b.HasBaseType("PerudoBot.Database.Sqlite.Data.Round");

                    b.HasDiscriminator().HasValue("StandardRound");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Action", b =>
                {
                    b.HasOne("PerudoBot.Database.Sqlite.Data.GamePlayer", "GamePlayer")
                        .WithMany("Actions")
                        .HasForeignKey("GamePlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PerudoBot.Database.Sqlite.Data.GamePlayerRound", "GamePlayerRound")
                        .WithMany("Actions")
                        .HasForeignKey("GamePlayerRoundId");

                    b.HasOne("PerudoBot.Database.Sqlite.Data.Action", "ParentAction")
                        .WithMany()
                        .HasForeignKey("ParentActionId");

                    b.HasOne("PerudoBot.Database.Sqlite.Data.Round", "Round")
                        .WithMany("Actions")
                        .HasForeignKey("RoundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GamePlayer");

                    b.Navigation("GamePlayerRound");

                    b.Navigation("ParentAction");

                    b.Navigation("Round");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.GamePlayer", b =>
                {
                    b.HasOne("PerudoBot.Database.Sqlite.Data.Game", "Game")
                        .WithMany("GamePlayers")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PerudoBot.Database.Sqlite.Data.Player", "Player")
                        .WithMany("GamesPlayed")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.GamePlayerRound", b =>
                {
                    b.HasOne("PerudoBot.Database.Sqlite.Data.GamePlayer", "GamePlayer")
                        .WithMany("GamePlayerRounds")
                        .HasForeignKey("GamePlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PerudoBot.Database.Sqlite.Data.Round", "Round")
                        .WithMany("GamePlayerRounds")
                        .HasForeignKey("RoundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GamePlayer");

                    b.Navigation("Round");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Round", b =>
                {
                    b.HasOne("PerudoBot.Database.Sqlite.Data.Game", "Game")
                        .WithMany("Rounds")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Game", b =>
                {
                    b.Navigation("GamePlayers");

                    b.Navigation("Rounds");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.GamePlayer", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("GamePlayerRounds");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.GamePlayerRound", b =>
                {
                    b.Navigation("Actions");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Player", b =>
                {
                    b.Navigation("GamesPlayed");
                });

            modelBuilder.Entity("PerudoBot.Database.Sqlite.Data.Round", b =>
                {
                    b.Navigation("Actions");

                    b.Navigation("GamePlayerRounds");
                });
#pragma warning restore 612, 618
        }
    }
}

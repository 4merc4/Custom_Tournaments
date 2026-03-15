using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Custom_Tournaments.Domain.Model;

namespace Custom_Tournaments_Infrastructure;

public partial class Custom_Tournaments_Context : DbContext
{
    public Custom_Tournaments_Context()
    {
    }

    public Custom_Tournaments_Context(DbContextOptions<Custom_Tournaments_Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Teammatchresult> Teammatchresults { get; set; }

    public virtual DbSet<Teammember> Teammembers { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<Tournamentparticipant> Tournamentparticipants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=game_turnirs_database;Username=postgres;Password=zero4444;");


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { 
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teams_pkey");

            entity.ToTable("teams");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Logourl).HasColumnName("logourl");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Ownerid).HasColumnName("ownerid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Owner).WithMany(p => p.Teams)
                .HasForeignKey(d => d.Ownerid)
                .HasConstraintName("fk_teams_owner");
        });

        modelBuilder.Entity<Teammatchresult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teammatchresults_pkey");

            entity.ToTable("teammatchresults");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Duration)
                .HasMaxLength(20)
                .HasColumnName("duration");
            entity.Property(e => e.Gamename)
                .HasMaxLength(100)
                .HasColumnName("gamename");
            entity.Property(e => e.Gamestats)
                .HasColumnType("jsonb")
                .HasColumnName("gamestats");
            entity.Property(e => e.Opponentname)
                .HasMaxLength(100)
                .HasColumnName("opponentname");
            entity.Property(e => e.Playedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("playedat");
            entity.Property(e => e.Result)
                .HasMaxLength(20)
                .HasColumnName("result");
            entity.Property(e => e.Score)
                .HasMaxLength(50)
                .HasColumnName("score");
            entity.Property(e => e.Teamid).HasColumnName("teamid");

            entity.HasOne(d => d.Team).WithMany(p => p.Teammatchresults)
                .HasForeignKey(d => d.Teamid)
                .HasConstraintName("fk_matchresults_team");
        });

        modelBuilder.Entity<Teammember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teammembers_pkey");

            entity.ToTable("teammembers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Joinedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joinedat");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Player'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.Teamid).HasColumnName("teamid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Team).WithMany(p => p.Teammembers)
                .HasForeignKey(d => d.Teamid)
                .HasConstraintName("fk_teammembers_team");

            entity.HasOne(d => d.User).WithMany(p => p.Teammembers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_teammembers_user");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tournaments_pkey");

            entity.ToTable("tournaments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Isprivate)
                .HasDefaultValue(false)
                .HasColumnName("isprivate");
            entity.Property(e => e.Organizerid).HasColumnName("organizerid");
            entity.Property(e => e.Prizepool)
                .HasPrecision(12, 2)
                .HasDefaultValue(0.00m)
                .HasColumnName("prizepool");
            entity.Property(e => e.Rules).HasColumnName("rules");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Tournaments)
                .HasForeignKey(d => d.Organizerid)
                .HasConstraintName("fk_tournaments_organizer");
        });

        modelBuilder.Entity<Tournamentparticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tournamentparticipants_pkey");

            entity.ToTable("tournamentparticipants");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Joinedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joinedat");
            entity.Property(e => e.Teamid).HasColumnName("teamid");
            entity.Property(e => e.Tournamentid).HasColumnName("tournamentid");

            entity.HasOne(d => d.Team).WithMany(p => p.Tournamentparticipants)
                .HasForeignKey(d => d.Teamid)
                .HasConstraintName("fk_participants_team");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Tournamentparticipants)
                .HasForeignKey(d => d.Tournamentid)
                .HasConstraintName("fk_participants_tournament");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Avatarurl).HasColumnName("avatarurl");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

public partial class TrackifyContext : DbContext
{
    public TrackifyContext()
    {
    }

    public TrackifyContext(DbContextOptions<TrackifyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActionType> ActionTypes { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<User> HolidayDTOs { get; set; }

    public virtual DbSet<UserLeave> UserLeaves { get; set; }

    public virtual DbSet<UserLeaveLog> UserLeaveLogs { get; set; }

    public virtual DbSet<UserLog> UserLogs { get; set; }

    public virtual DbSet<UserProject> UserProjects { get; set; }

    public virtual DbSet<UserProjectLog> UserProjectLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Trackify;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ActionTy__3214EC071E0533B8");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Holiday__3214EC07B61D74C8");

            entity.Property(e => e.Id).ValueGeneratedNever();

            //entity.HasOne(d => d.Location).WithMany(p => p.Holidays)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_Event_Location");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveTyp__3214EC07B35FFAFF");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC07404D05F6");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Project__3214EC0767FCC81A");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC0708443879");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC075E147F66");

            entity.HasOne(d => d.ActionType).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_ActionType");

            //entity.HasOne(d => d.Location).WithMany(p => p.Users)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_User_Location");
        });

        modelBuilder.Entity<UserLeave>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserLeav__3214EC07C0133B4E");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.ActionType).WithMany(p => p.UserLeaves)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserLeave_ActionType");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.UserLeave)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Id");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.UserLeaves)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveType");
        });

        modelBuilder.Entity<UserLeaveLog>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<UserProject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserProj__3214EC07842D809E");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ActionType).WithMany(p => p.UserProjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProject_ActionType");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.UserProject)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProject_UserId");

            entity.HasOne(d => d.Project).WithMany(p => p.UserProjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProject_PRojectId");

            entity.HasOne(d => d.Role).WithMany(p => p.UserProjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProject_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

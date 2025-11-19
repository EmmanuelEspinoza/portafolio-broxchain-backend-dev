using System;
using System.Collections.Generic;
using FondoInversion.Models;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<accesoUser> accesoUsers { get; set; }

    public virtual DbSet<adminUser> adminUsers { get; set; }

    public virtual DbSet<eventLog> eventLogs { get; set; }

    public virtual DbSet<flujo> flujos { get; set; }

    public virtual DbSet<precio> precios { get; set; }

    public virtual DbSet<token> tokens { get; set; }

    public virtual DbSet<transactionLog> transactionLogs { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<accesoUser>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__accesoUs__3213E83FA656F1CD");

            entity.HasOne(d => d.user).WithMany(p => p.accesoUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__accesoUse__userI__59FA5E80");
        });

        modelBuilder.Entity<adminUser>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__adminUse__3213E83F2761EE0B");
        });

        modelBuilder.Entity<eventLog>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__eventLog__3213E83F329D075A");
        });

        modelBuilder.Entity<flujo>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__flujo__3213E83F399E913D");

            entity.Property(e => e.id).ValueGeneratedNever();

            entity.HasOne(d => d.inversionista).WithMany(p => p.flujos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__flujo__inversion__4CA06362");
        });

        modelBuilder.Entity<precio>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__precio__3213E83F7DEC59F5");
        });

        modelBuilder.Entity<token>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__tokens__3213E83FFD91BCFC");

            entity.HasOne(d => d.user).WithMany(p => p.tokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tokens__userID__72C60C4A");
        });

        modelBuilder.Entity<transactionLog>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__transact__3213E83FA47BDB6D");

            entity.HasOne(d => d.user).WithMany(p => p.transactionLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__userI__5535A963");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__user__3213E83F99A70B0B");

            entity.Property(e => e.id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

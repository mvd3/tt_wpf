using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TeleTrader.Database;

public partial class DB : DbContext
{
    private string _path;

    public DB(DbContextOptions<DB> options)
        : base(options)
    {
    }

    public DB(string path)
    {
        _path = path;
    }

    public virtual DbSet<Exchange> Exchanges { get; set; }

    public virtual DbSet<Symbol> Symbols { get; set; }

    public virtual DbSet<Type> Types { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={_path}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exchange>(entity =>
        {
            entity.ToTable("Exchange");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnType("nvarchar(254)");
        });

        modelBuilder.Entity<Symbol>(entity =>
        {
            entity.ToTable("Symbol");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CurrencyCode).HasColumnType("char(3)");
            entity.Property(e => e.DateAdded).HasColumnType("date");
            entity.Property(e => e.ExchangeId).HasColumnType("INT");
            entity.Property(e => e.Isin).HasColumnType("char(13)");
            entity.Property(e => e.Name).HasColumnType("nvarchar(254)");
            entity.Property(e => e.Price).HasColumnType("double");
            entity.Property(e => e.PriceDate).HasColumnType("date");
            entity.Property(e => e.Ticker).HasColumnType("nvarchar(120)");
            entity.Property(e => e.TypeId).HasColumnType("INT");

            entity.HasOne(d => d.Exchange).WithMany(p => p.Symbols)
                .HasForeignKey(d => d.ExchangeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Type).WithMany(p => p.Symbols)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Type>(entity =>
        {
            entity.ToTable("Type");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnType("nvarchar(254)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

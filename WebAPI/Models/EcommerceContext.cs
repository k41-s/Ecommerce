using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models;

public partial class EcommerceContext : DbContext
{
    public EcommerceContext()
    {
    }

    public EcommerceContext(DbContextOptions<EcommerceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<CustomerOrder> CustomerOrders { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC0791FFCFE4");

            entity.ToTable("Category");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Country__3214EC07ACC70C5B");

            entity.ToTable("Country");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07F9E20E53");

            entity.ToTable("CustomerOrder");

            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.OrderedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("CreditCard");

            entity.HasOne(d => d.Product).WithMany(p => p.CustomerOrders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerO__Produ__35BCFE0A");

            entity.HasOne(d => d.User).WithMany(p => p.CustomerOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerO__UserI__36B12243");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Log__3214EC078020C8D4");

            entity.ToTable("Log");

            entity.Property(e => e.Level).HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC072079486F");

            entity.ToTable("Product");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__2D27B809");

            entity.HasMany(d => d.Countries).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductCountry",
                    r => r.HasOne<Country>().WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductCo__Count__30F848ED"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductCo__Produ__300424B4"),
                    j =>
                    {
                        j.HasKey("ProductId", "CountryId").HasName("PK__ProductC__5501D0C4AA7D4612");
                        j.ToTable("ProductCountry");
                    });
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductI__3214EC074F558E46");

            entity.ToTable("ProductImage");

            entity.Property(e => e.MimeType).HasMaxLength(100);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductIm__Produ__398D8EEE");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC077DD6E8FD");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4F8DCBFE0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053422FCCCE8").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(10);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Surname).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(15);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

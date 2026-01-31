using GS1.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GS1.Infrastructure.Data;


/// Product entity configuration

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.GTIN)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // GTIN benzersiz olmalı
        builder.HasIndex(p => p.GTIN)
            .IsUnique()
            .HasDatabaseName("IX_Products_GTIN");

        builder.HasIndex(p => p.ProductName)
            .HasDatabaseName("IX_Products_ProductName");

        builder.HasIndex(p => p.CustomerId)
            .HasDatabaseName("IX_Products_CustomerId");

        // Foreign Key - Customer
        builder.HasOne(p => p.Customer)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete için filtre
        builder.HasQueryFilter(p => p.IsActive);
    }
}

using GS1.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GS1.Infrastructure.Data;


/// Customer entity configuration

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.GLN)
            .IsRequired()
            .HasMaxLength(13)
            .IsFixedLength();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // GLN benzersiz olmalı
        builder.HasIndex(c => c.GLN)
            .IsUnique()
            .HasDatabaseName("IX_Customers_GLN");

        builder.HasIndex(c => c.CompanyName)
            .HasDatabaseName("IX_Customers_CompanyName");

        // Soft delete için filtre
        builder.HasQueryFilter(c => c.IsActive);
    }
}

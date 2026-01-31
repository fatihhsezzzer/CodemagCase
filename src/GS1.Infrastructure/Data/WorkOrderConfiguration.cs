using GS1.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GS1.Infrastructure.Data;


/// WorkOrder entity configuration

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WorkOrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.ProductionQuantity)
            .IsRequired();

        builder.Property(w => w.BatchNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(w => w.ExpirationDate)
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired()
            .HasDefaultValue(WorkOrderStatus.Created);

        builder.Property(w => w.ItemsPerBox)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(w => w.BoxesPerPallet)
            .IsRequired()
            .HasDefaultValue(100);

        // İş emri numarası benzersiz olmalı
        builder.HasIndex(w => w.WorkOrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_WorkOrders_WorkOrderNumber");

        builder.HasIndex(w => w.ProductId)
            .HasDatabaseName("IX_WorkOrders_ProductId");

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("IX_WorkOrders_Status");

        builder.HasIndex(w => w.BatchNumber)
            .HasDatabaseName("IX_WorkOrders_BatchNumber");

        // Foreign Key - Product
        builder.HasOne(w => w.Product)
            .WithMany(p => p.WorkOrders)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete için filtre
        builder.HasQueryFilter(w => w.IsActive);
    }
}

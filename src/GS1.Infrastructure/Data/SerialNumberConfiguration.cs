using GS1.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GS1.Infrastructure.Data;


/// SerialNumber entity configuration

public class SerialNumberConfiguration : IEntityTypeConfiguration<SerialNumber>
{
    public void Configure(EntityTypeBuilder<SerialNumber> builder)
    {
        builder.ToTable("SerialNumbers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Serial)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.GS1DataMatrix)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasDefaultValue(SerialNumberStatus.Generated);

        // İş emri içinde seri numarası benzersiz olmalı
        builder.HasIndex(s => new { s.WorkOrderId, s.Serial })
            .IsUnique()
            .HasDatabaseName("IX_SerialNumbers_WorkOrderId_Serial");

        builder.HasIndex(s => s.WorkOrderId)
            .HasDatabaseName("IX_SerialNumbers_WorkOrderId");

        builder.HasIndex(s => s.SSCCId)
            .HasDatabaseName("IX_SerialNumbers_SSCCId");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_SerialNumbers_Status");

        // Foreign Key - WorkOrder
        builder.HasOne(s => s.WorkOrder)
            .WithMany(w => w.SerialNumbers)
            .HasForeignKey(s => s.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign Key - SSCC (Nullable)
        builder.HasOne(s => s.SSCC)
            .WithMany(sc => sc.SerialNumbers)
            .HasForeignKey(s => s.SSCCId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

using GS1.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GS1.Infrastructure.Data;


/// SSCC entity configuration

public class SSCCConfiguration : IEntityTypeConfiguration<SSCC>
{
    public void Configure(EntityTypeBuilder<SSCC> builder)
    {
        builder.ToTable("SSCCs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SSCCCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Type)
            .IsRequired();

        builder.Property(s => s.GS1DataMatrix)
            .IsRequired()
            .HasMaxLength(100);

        // SSCC kodu global olarak benzersiz olmalı
        builder.HasIndex(s => s.SSCCCode)
            .IsUnique()
            .HasDatabaseName("IX_SSCCs_SSCCCode");

        builder.HasIndex(s => s.WorkOrderId)
            .HasDatabaseName("IX_SSCCs_WorkOrderId");

        builder.HasIndex(s => s.ParentSSCCId)
            .HasDatabaseName("IX_SSCCs_ParentSSCCId");

        builder.HasIndex(s => s.Type)
            .HasDatabaseName("IX_SSCCs_Type");

        // Foreign Key - WorkOrder
        builder.HasOne(s => s.WorkOrder)
            .WithMany(w => w.SSCCs)
            .HasForeignKey(s => s.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing Foreign Key - Parent SSCC (Palet-Koli ilişkisi)
        builder.HasOne(s => s.ParentSSCC)
            .WithMany(s => s.ChildSSCCs)
            .HasForeignKey(s => s.ParentSSCCId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

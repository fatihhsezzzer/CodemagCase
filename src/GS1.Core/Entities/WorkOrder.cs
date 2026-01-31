using System.ComponentModel.DataAnnotations;

namespace GS1.Core.Entities;


/// İş Emri (Work Order) entity'si
/// Üretim iş emri bilgilerini ve GS1 parametrelerini tutar

public class WorkOrder : BaseEntity
{
    
    /// İş Emri Numarası
    
    [Required]
    [MaxLength(50)]
    public string WorkOrderNumber { get; set; } = string.Empty;

    
    /// Üretim Adedi
    
    [Required]
    public int ProductionQuantity { get; set; }

    
    /// Lot / Batch No (10)
    /// GS1 AI(10) - Batch/Lot Number
    
    [Required]
    [MaxLength(20)]
    public string BatchNumber { get; set; } = string.Empty;

    
    /// Son Kullanma Tarihi (17)
    /// GS1 AI(17) - Expiration Date (YYMMDD format)
    
    [Required]
    public DateTime ExpirationDate { get; set; }

    
    /// Seri numarası başlangıç değeri
    
    public long SerialNumberStart { get; set; } = 1;

    
    /// Son kullanılan seri numarası
    
    public long LastSerialNumber { get; set; } = 0;

    
    /// İş emri durumu
    
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;

    
    /// Ürün ID (Foreign Key)
    
    public Guid ProductId { get; set; }

    
    /// Koli başına ürün adedi
    
    public int ItemsPerBox { get; set; } = 10;

    
    /// Palet başına koli adedi
    
    public int BoxesPerPallet { get; set; } = 100;

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
    public virtual ICollection<SSCC> SSCCs { get; set; } = new List<SSCC>();
}

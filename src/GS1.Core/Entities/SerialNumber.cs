using System.ComponentModel.DataAnnotations;

namespace GS1.Core.Entities;


/// Seri Numarası entity'si
/// GS1 AI(21) Serial Number bilgilerini tutar

public class SerialNumber : BaseEntity
{
    
    /// Seri Numarası (21)
    /// GS1 AI(21) - Serial Number (max 20 karakter)
    
    [Required]
    [MaxLength(20)]
    public string Serial { get; set; } = string.Empty;

    
    /// Tam GS1 Data Matrix string'i
    /// Format: (01)GTIN(21)Serial(17)ExpDate(10)Batch
    
    [MaxLength(100)]
    public string GS1DataMatrix { get; set; } = string.Empty;

    
    /// Seri numarası durumu
    
    public SerialNumberStatus Status { get; set; } = SerialNumberStatus.Generated;

    
    /// Basım zamanı
    
    public DateTime? PrintedAt { get; set; }

    
    /// Doğrulama zamanı
    
    public DateTime? VerifiedAt { get; set; }

    
    /// İş Emri ID (Foreign Key)
    
    public Guid WorkOrderId { get; set; }

    
    /// SSCC ID (Koli bağlantısı - Nullable)
    
    public Guid? SSCCId { get; set; }

    // Navigation Properties
    public virtual WorkOrder WorkOrder { get; set; } = null!;
    public virtual SSCC? SSCC { get; set; }
}

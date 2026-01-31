using System.ComponentModel.DataAnnotations;

namespace GS1.Core.Entities;


/// SSCC (Serial Shipping Container Code) entity'si
/// GS1 AI(00) - Lojistik birim tanımlayıcısı
/// Ürün → Koli → Palet agregasyonu için kullanılır

public class SSCC : BaseEntity
{
    
    /// SSCC Kodu (00)
    /// GS1 AI(00) - 18 haneli SSCC kodu
    
    [Required]
    [StringLength(18, MinimumLength = 18)]
    public string SSCCCode { get; set; } = string.Empty;

    
    /// SSCC Tipi (Koli veya Palet)
    
    public SSCCType Type { get; set; }

    
    /// İş Emri ID (Foreign Key)
    
    public Guid WorkOrderId { get; set; }

    
    /// Üst SSCC ID (Palet için null, Koli için bağlı olduğu palet)
    
    public Guid? ParentSSCCId { get; set; }

    
    /// GS1 Data Matrix string'i
    
    [MaxLength(100)]
    public string GS1DataMatrix { get; set; } = string.Empty;

    // Navigation Properties
    public virtual WorkOrder WorkOrder { get; set; } = null!;
    public virtual SSCC? ParentSSCC { get; set; }
    
    
    /// Alt SSCC'ler (Palet için koliler)
    
    public virtual ICollection<SSCC> ChildSSCCs { get; set; } = new List<SSCC>();
    
    
    /// İçerdiği seri numaraları (Koli için ürünler)
    
    public virtual ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
}

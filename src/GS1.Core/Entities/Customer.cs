using System.ComponentModel.DataAnnotations;

namespace GS1.Core.Entities;


/// Müşteri entity'si
/// GLN (Global Location Number) ile tanımlanan firma bilgilerini tutar

public class Customer : BaseEntity
{
    
    /// Firma Adı
    
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    
    /// GLN (Global Location Number) - 13 haneli
    /// GS1 standardına göre benzersiz firma tanımlayıcısı
    
    [Required]
    [StringLength(13, MinimumLength = 13)]
    public string GLN { get; set; } = string.Empty;

    
    /// Açıklama
    
    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

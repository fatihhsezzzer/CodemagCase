using System.ComponentModel.DataAnnotations;

namespace GS1.Core.Entities;


/// Ürün entity'si
/// GTIN (Global Trade Item Number) ile tanımlanan ürün bilgilerini tutar

public class Product : BaseEntity
{
    
    /// GTIN (01) - Global Trade Item Number - 14 haneli
    /// GS1 standardına göre benzersiz ürün tanımlayıcısı
    
    [Required]
    [StringLength(14, MinimumLength = 8)]
    public string GTIN { get; set; } = string.Empty;

    
    /// Ürün Adı
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    
    /// Ürün Açıklaması
    
    [MaxLength(500)]
    public string? Description { get; set; }

    
    /// Müşteri ID (Foreign Key)
    
    public Guid CustomerId { get; set; }

    // Navigation Properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

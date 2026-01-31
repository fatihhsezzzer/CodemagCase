namespace GS1.Core.Entities;


/// Seri numarası durumu

public enum SerialNumberStatus
{
    
    /// Üretildi, henüz basılmadı
    
    Generated = 0,

    
    /// Basıldı
    
    Printed = 1,

    
    /// Doğrulandı 
    
    Verified = 2,

    
    /// Reddedildi (doğrulama başarısız)
    
    Rejected = 3,

    
    /// Agregasyona dahil edildi
    
    Aggregated = 4
}

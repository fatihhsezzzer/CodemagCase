namespace GS1.Core.Entities;


/// İş emri durumları

public enum WorkOrderStatus
{
    
    /// Oluşturuldu, henüz başlamadı
    
    Created = 0,

    
    /// Üretim devam ediyor
    
    InProgress = 1,

    
    /// Üretim tamamlandı
    
    Completed = 2,

    
    /// İptal edildi
    
    Cancelled = 3,

    
    /// Beklemede
    
    OnHold = 4
}

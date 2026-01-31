using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// Agregasyon servisi interface'i
/// Ürün → Koli → Palet agregasyonu

public interface IAggregationService
{
    
    /// Yeni koli SSCC'si oluşturur
    
    Task<SSCC> CreateBoxAsync(Guid workOrderId);

    
    /// Yeni palet SSCC'si oluşturur
    
    Task<SSCC> CreatePalletAsync(Guid workOrderId);

    
    /// Ürünleri (seri numaralarını) koliye ekler
    
    Task AddItemsToBoxAsync(Guid boxId, IEnumerable<Guid> serialNumberIds);

    
    /// Kolileri palete ekler
    
    Task AddBoxesToPalletAsync(Guid palletId, IEnumerable<Guid> boxIds);

    
    /// Ürünü koliden çıkarır
    
    Task RemoveItemFromBoxAsync(Guid serialNumberId);

    
    /// Koliyi paletten çıkarır
    
    Task RemoveBoxFromPalletAsync(Guid boxId);

    
    /// Agregasyon hiyerarşisini getirir
    
    Task<SSCC?> GetAggregationHierarchyAsync(Guid ssccId);
}

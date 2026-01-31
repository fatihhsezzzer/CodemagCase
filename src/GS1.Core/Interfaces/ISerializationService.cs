using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// Serilizasyon servisi interface'i
/// Seri numarası üretimi ve yönetimi

public interface ISerializationService
{
    
    /// İş emri için seri numaraları üretir
    
    Task<IEnumerable<SerialNumber>> GenerateSerialNumbersAsync(Guid workOrderId, int quantity);

    
    /// Seri numarasını basıldı olarak işaretler
    
    Task MarkAsPrintedAsync(Guid serialNumberId);

    
    /// Seri numarasını doğrulandı olarak işaretler
    
    Task MarkAsVerifiedAsync(Guid serialNumberId);

    
    /// Seri numarasını reddedildi olarak işaretler
    
    Task MarkAsRejectedAsync(Guid serialNumberId);
}

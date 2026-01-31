using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// GS1 Identifier üretim servisi interface'i

public interface IGS1GeneratorService
{
    
    /// GS1 Data Matrix string'i oluşturur
    /// Format: (01)GTIN(21)Serial(17)ExpDate(10)Batch
    
    string GenerateGS1DataMatrix(string gtin, string serial, DateTime expirationDate, string batchNumber);

    
    /// Benzersiz seri numarası üretir
    
    string GenerateSerialNumber(long sequenceNumber, string? prefix = null);

    
    /// SSCC (00) kodu üretir - 18 haneli
    
    string GenerateSSCC(string companyPrefix, long extensionDigit, long serialReference);

    
    /// SSCC için GS1 Data Matrix string'i oluşturur
    
    string GenerateSSCCDataMatrix(string sscc);

    
    /// GTIN check digit'i hesaplar ve doğrular
    
    bool ValidateGTIN(string gtin);

    
    /// GLN check digit'i hesaplar ve doğrular
    
    bool ValidateGLN(string gln);

    
    /// SSCC check digit'i hesaplar ve doğrular
    
    bool ValidateSSCC(string sscc);

    
    /// GS1-128 barkod formatında expiration date formatlar (YYMMDD)
    
    string FormatExpirationDate(DateTime date);

    
    /// Check digit hesaplar (mod10 algoritması)
    
    int CalculateCheckDigit(string digits);
}

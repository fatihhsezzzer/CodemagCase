using GS1.Core.Interfaces;

namespace GS1.Infrastructure.Services;


/// GS1 Identifier üretim servisi implementasyonu
/// GS1 standartlarına uygun AI (Application Identifier) üretimi

public class GS1GeneratorService : IGS1GeneratorService
{
    // GS1 Group Separator (ASCII 29) - FNC1 karakteri yerine kullanılır
    private const char GS = '\u001D';
    
    
    /// GS1 Data Matrix string'i oluşturur
    /// Format: (01)GTIN(17)ExpDate(10)Batch(21)Serial
    
    public string GenerateGS1DataMatrix(string gtin, string serial, DateTime expirationDate, string batchNumber)
    {
        // GTIN'i 14 haneye tamamla
        var paddedGtin = gtin.PadLeft(14, '0');
        
        // Expiration date formatı: YYMMDD
        var expDate = FormatExpirationDate(expirationDate);
        
        // GS1 Element String oluştur
        // AI(01) = GTIN (14 haneli - sabit uzunluk, GS gerekmez)
        // AI(17) = Expiration Date (6 haneli - sabit uzunluk, GS gerekmez)
        // AI(10) = Batch/Lot (değişken uzunluk, sonunda GS gerekir)
        // AI(21) = Serial (değişken uzunluk, sonunda GS gerekmez - son eleman)
        
        return $"01{paddedGtin}17{expDate}10{batchNumber}{GS}21{serial}";
    }

    
    /// Benzersiz seri numarası üretir
    
    public string GenerateSerialNumber(long sequenceNumber, string? prefix = null)
    {
        // GS1 AI(21) max 20 karakter
        var serial = sequenceNumber.ToString().PadLeft(10, '0');
        
        if (!string.IsNullOrEmpty(prefix))
        {
            // Prefix ile birlikte max 20 karakter
            var maxSerialLength = 20 - prefix.Length;
            serial = sequenceNumber.ToString().PadLeft(maxSerialLength, '0');
            return $"{prefix}{serial}";
        }
        
        return serial;
    }

    
    /// SSCC (00) kodu üretir - 18 haneli
    /// Format: Extension Digit (1) + GS1 Company Prefix + Serial Reference + Check Digit
    
    public string GenerateSSCC(string companyPrefix, long extensionDigit, long serialReference)
    {
        // Extension digit (1 hane) + Company Prefix + Serial Reference = 17 hane
        // Son hane check digit
        
        // Company prefix'i temizle (sadece rakamlar)
        var cleanPrefix = new string(companyPrefix.Where(char.IsDigit).ToArray());
        
        // Serial reference'ı hesapla - toplam 17 hane olacak şekilde
        var serialLength = 17 - 1 - cleanPrefix.Length; // 1 = extension digit
        var serialStr = serialReference.ToString().PadLeft(serialLength, '0');
        
        // 17 haneli SSCC (check digit hariç)
        var sscc17 = $"{extensionDigit}{cleanPrefix}{serialStr}";
        
        // Check digit hesapla
        var checkDigit = CalculateCheckDigit(sscc17);
        
        return $"{sscc17}{checkDigit}";
    }

    
    /// SSCC için GS1 Data Matrix string'i oluşturur
    
    public string GenerateSSCCDataMatrix(string sscc)
    {
        return $"00{sscc}";
    }

    
    /// GTIN check digit'i hesaplar ve doğrular
    /// GTIN-8, GTIN-12, GTIN-13, GTIN-14 destekler
    
    public bool ValidateGTIN(string gtin)
    {
        if (string.IsNullOrEmpty(gtin))
            return false;

        // Sadece rakamlardan oluşmalı
        if (!gtin.All(char.IsDigit))
            return false;

        // Desteklenen uzunluklar: 8, 12, 13, 14
        if (gtin.Length != 8 && gtin.Length != 12 && gtin.Length != 13 && gtin.Length != 14)
            return false;

        // GTIN Numarasının Son hanesini alır
        var checkDigit = int.Parse(gtin[^1].ToString());
        //Son hane dışındaki kısmı alır ve fonksiyona gönderir
        var calculatedCheckDigit = CalculateCheckDigit(gtin[..^1]);
        // Hesaplanan check digit ile verilen check digit'i karşılaştırır
        return checkDigit == calculatedCheckDigit;
    }

    
    /// GLN check digit'i hesaplar ve doğrular
    /// GLN 13 haneli olmalı
    
    public bool ValidateGLN(string gln)
    {
        if (string.IsNullOrEmpty(gln))
            return false;

        // Sadece rakamlardan oluşmalı
        if (!gln.All(char.IsDigit))
            return false;

        // GLN 13 haneli
        if (gln.Length != 13)
            return false;

        // GTIN Numarasının Son hanesini alır
        var checkDigit = int.Parse(gln[^1].ToString());
        //Son hane dışındaki kısmı alır ve fonksiyona gönderir
        var calculatedCheckDigit = CalculateCheckDigit(gln[..^1]);
        // Hesaplanan check digit ile verilen check digit'i karşılaştırır
        return checkDigit == calculatedCheckDigit;
    }

    
    /// SSCC check digit'i hesaplar ve doğrular
    /// SSCC 18 haneli olmalı
    
    public bool ValidateSSCC(string sscc)
    {
        if (string.IsNullOrEmpty(sscc))
            return false;

        // Sadece rakamlardan oluşmalı
        if (!sscc.All(char.IsDigit))
            return false;

        // SSCC 18 haneli
        if (sscc.Length != 18)
            return false;

        // GTIN Numarasının Son hanesini alır
        var checkDigit = int.Parse(sscc[^1].ToString());
        //Son hane dışındaki kısmı alır ve fonksiyona gönderir
        var calculatedCheckDigit = CalculateCheckDigit(sscc[..^1]);
        
        // Hesaplanan check digit ile verilen check digit'i karşılaştırır
        return checkDigit == calculatedCheckDigit;
    }

    
    /// GS1-128 barkod formatında expiration date formatlar (YYMMDD)
    
    public string FormatExpirationDate(DateTime date)
    {
        return date.ToString("yyMMdd");
    }

    
    /// Check digit hesaplaması (GS1 Mod10 algoritması ile)
    
    public int CalculateCheckDigit(string digits)
    {
        if (string.IsNullOrEmpty(digits) || !digits.All(char.IsDigit))
            throw new ArgumentException("Geçersiz digit string", nameof(digits));

        var sum = 0;
        var isOdd = true; // Sağdan başlayarak tek pozisyonlar alınır

        // Sağdan sola doğru işle
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var digit = int.Parse(digits[i].ToString());
            
            if (isOdd)
                sum += digit * 3;
            else
                sum += digit * 1;
            
            isOdd = !isOdd;
        }

      // Digit Kontrolü toplam 10 un katı olmamalı
        var checkDigit = (10 - (sum % 10)) % 10;
        
        return checkDigit;
    }
}

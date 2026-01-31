using GS1.Core.Entities;

namespace GS1.Core.DTOs;

// ================== Work Order DTOs ==================

public record WorkOrderDto(
    Guid Id,
    string WorkOrderNumber,
    int ProductionQuantity,
    string BatchNumber,
    DateTime ExpirationDate,
    long SerialNumberStart,
    long LastSerialNumber,
    WorkOrderStatus Status,
    string StatusText,
    Guid ProductId,
    string? ProductName,
    string? GTIN,
    int ItemsPerBox,
    int BoxesPerPallet,
    DateTime CreatedAt,
    bool IsActive
);

public record WorkOrderCreateDto(
    string WorkOrderNumber,
    int ProductionQuantity,
    string BatchNumber,
    DateTime ExpirationDate,
    long SerialNumberStart,
    Guid ProductId,
    int ItemsPerBox = 10,
    int BoxesPerPallet = 100
);

public record WorkOrderUpdateDto(
    string WorkOrderNumber,
    int ProductionQuantity,
    string BatchNumber,
    DateTime ExpirationDate,
    WorkOrderStatus Status,
    int ItemsPerBox,
    int BoxesPerPallet,
    bool IsActive
);


/// İş emri detay response - Tüm bilgileri içerir

public record WorkOrderDetailDto(
    // İş emri bilgileri
    Guid Id,
    string WorkOrderNumber,
    int ProductionQuantity,
    string BatchNumber,
    DateTime ExpirationDate,
    long SerialNumberStart,
    long LastSerialNumber,
    WorkOrderStatus Status,
    string StatusText,
    int ItemsPerBox,
    int BoxesPerPallet,
    DateTime CreatedAt,
    
    // Ürün bilgileri
    ProductDto Product,
    
    // Müşteri bilgileri
    CustomerDto Customer,
    
    // Üretilmiş seri numaraları
    List<SerialNumberDto> SerialNumbers,
    
    // SSCC ve agregasyon yapısı
    List<SSCCAggregationDto> Aggregations,
    
    // Özet bilgiler
    WorkOrderSummaryDto Summary
);

public record WorkOrderSummaryDto(
    int TotalSerialNumbers,
    int PrintedCount,
    int VerifiedCount,
    int RejectedCount,
    int AggregatedCount,
    int TotalBoxes,
    int TotalPallets,
    double CompletionPercentage
);

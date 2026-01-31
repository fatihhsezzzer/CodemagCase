using GS1.Core.Entities;

namespace GS1.Core.DTOs;

// ================== SSCC DTOs ==================

public record SSCCDto(
    Guid Id,
    string SSCCCode,
    SSCCType Type,
    string TypeText,
    string GS1DataMatrix,
    Guid WorkOrderId,
    Guid? ParentSSCCId,
    string? ParentSSCCCode,
    int ItemCount,
    int? ItemsPerBox,
    DateTime CreatedAt
);

public record SSCCCreateDto(
    Guid WorkOrderId,
    SSCCType Type
);


/// Agregasyon hiyerarşisi - Palet → Koliler → Ürünler

public record SSCCAggregationDto(
    Guid Id,
    string SSCCCode,
    SSCCType Type,
    string TypeText,
    string GS1DataMatrix,
    Guid? ParentSSCCId,
    
    // Koli içindeki ürünler (sadece Box tipinde dolu)
    List<SerialNumberDto>? Items,
    
    // Palet içindeki koliler (sadece Pallet tipinde dolu)
    List<SSCCAggregationDto>? ChildBoxes
);

public record AddItemsToBoxDto(
    Guid BoxId,
    List<Guid> SerialNumberIds
);

public record AddBoxesToPalletDto(
    Guid PalletId,
    List<Guid> BoxIds
);

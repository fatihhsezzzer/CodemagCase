using GS1.Core.Entities;

namespace GS1.Core.DTOs;

// ================== Serial Number DTOs ==================

public record SerialNumberDto(
    Guid Id,
    string Serial,
    string GS1DataMatrix,
    SerialNumberStatus Status,
    string StatusText,
    DateTime? PrintedAt,
    DateTime? VerifiedAt,
    Guid WorkOrderId,
    Guid? SSCCId,
    string? SSCCCode,
    DateTime CreatedAt
);

public record SerialNumberGenerateDto(
    Guid WorkOrderId,
    int Quantity
);

public record SerialNumberStatusUpdateDto(
    SerialNumberStatus Status
);

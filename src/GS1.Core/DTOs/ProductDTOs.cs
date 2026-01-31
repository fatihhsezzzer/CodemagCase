namespace GS1.Core.DTOs;

// ================== Product DTOs ==================

public record ProductDto(
    Guid Id,
    string GTIN,
    string ProductName,
    string? Description,
    Guid CustomerId,
    string? CustomerName,
    DateTime CreatedAt,
    bool IsActive
);

public record ProductCreateDto(
    string GTIN,
    string ProductName,
    string? Description,
    Guid CustomerId
);

public record ProductUpdateDto(
    string GTIN,
    string ProductName,
    string? Description,
    Guid CustomerId,
    bool IsActive
);

public record ProductWithWorkOrdersDto(
    Guid Id,
    string GTIN,
    string ProductName,
    string? Description,
    Guid CustomerId,
    string? CustomerName,
    DateTime CreatedAt,
    bool IsActive,
    List<WorkOrderDto> WorkOrders
);

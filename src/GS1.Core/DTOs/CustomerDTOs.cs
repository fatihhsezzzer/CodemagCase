namespace GS1.Core.DTOs;

// ================== Customer DTOs ==================

public record CustomerDto(
    Guid Id,
    string CompanyName,
    string GLN,
    string? Description,
    DateTime CreatedAt,
    bool IsActive
);

public record CustomerCreateDto(
    string CompanyName,
    string GLN,
    string? Description
);

public record CustomerUpdateDto(
    string CompanyName,
    string GLN,
    string? Description,
    bool IsActive
);

public record CustomerWithProductsDto(
    Guid Id,
    string CompanyName,
    string GLN,
    string? Description,
    DateTime CreatedAt,
    bool IsActive,
    List<ProductDto> Products
);

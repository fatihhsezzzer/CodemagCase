using GS1.Core.DTOs;
using GS1.Core.Entities;
using GS1.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GS1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGS1GeneratorService _gs1Generator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IUnitOfWork unitOfWork,
        IGS1GeneratorService gs1Generator,
        ILogger<ProductsController> logger)
    {
        _unitOfWork = unitOfWork;
        _gs1Generator = gs1Generator;
        _logger = logger;
    }

    
    /// Tüm ürünleri listeler
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        
        var dtos = products.Select(p => new ProductDto(
            p.Id,
            p.GTIN,
            p.ProductName,
            p.Description,
            p.CustomerId,
            p.Customer?.CompanyName,
            p.CreatedAt,
            p.IsActive
        ));

        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(dtos));
    }

    
    /// ID'ye göre ürün getirir
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        
        if (product == null)
            return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Ürün bulunamadı. ID: {id}"));

        var customer = await _unitOfWork.Customers.GetByIdAsync(product.CustomerId);

        var dto = new ProductDto(
            product.Id,
            product.GTIN,
            product.ProductName,
            product.Description,
            product.CustomerId,
            customer?.CompanyName,
            product.CreatedAt,
            product.IsActive
        );

        return Ok(ApiResponse<ProductDto>.SuccessResponse(dto));
    }

    
    /// Müşteriye göre ürünleri listeler
    
    [HttpGet("by-customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCustomer(Guid customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        
        if (customer == null)
            return NotFound(ApiResponse<IEnumerable<ProductDto>>.ErrorResponse($"Müşteri bulunamadı. ID: {customerId}"));

        var products = await _unitOfWork.Products.GetByCustomerIdAsync(customerId);
        
        var dtos = products.Select(p => new ProductDto(
            p.Id,
            p.GTIN,
            p.ProductName,
            p.Description,
            p.CustomerId,
            customer.CompanyName,
            p.CreatedAt,
            p.IsActive
        ));

        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(dtos));
    }

    
    /// Ürün ve iş emirlerini getirir
    
    [HttpGet("{id}/with-workorders")]
    public async Task<ActionResult<ApiResponse<ProductWithWorkOrdersDto>>> GetWithWorkOrders(Guid id)
    {
        var product = await _unitOfWork.Products.GetWithWorkOrdersAsync(id);
        
        if (product == null)
            return NotFound(ApiResponse<ProductWithWorkOrdersDto>.ErrorResponse($"Ürün bulunamadı. ID: {id}"));

        var dto = new ProductWithWorkOrdersDto(
            product.Id,
            product.GTIN,
            product.ProductName,
            product.Description,
            product.CustomerId,
            product.Customer?.CompanyName,
            product.CreatedAt,
            product.IsActive,
            product.WorkOrders.Select(w => new WorkOrderDto(
                w.Id,
                w.WorkOrderNumber,
                w.ProductionQuantity,
                w.BatchNumber,
                w.ExpirationDate,
                w.SerialNumberStart,
                w.LastSerialNumber,
                w.Status,
                w.Status.ToString(),
                w.ProductId,
                product.ProductName,
                product.GTIN,
                w.ItemsPerBox,
                w.BoxesPerPallet,
                w.CreatedAt,
                w.IsActive
            )).ToList()
        );

        return Ok(ApiResponse<ProductWithWorkOrdersDto>.SuccessResponse(dto));
    }

    
    /// Yeni ürün oluşturur
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] ProductCreateDto dto)
    {
        // GTIN validasyonu
        if (!_gs1Generator.ValidateGTIN(dto.GTIN))
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse($"Geçersiz GTIN formatı: {dto.GTIN}"));

        // GTIN benzersizlik kontrolü
        if (await _unitOfWork.Products.GTINExistsAsync(dto.GTIN))
            return Conflict(ApiResponse<ProductDto>.ErrorResponse($"Bu GTIN zaten kayıtlı: {dto.GTIN}"));

        // Müşteri varlık kontrolü
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Müşteri bulunamadı. ID: {dto.CustomerId}"));

        var product = new Product
        {
            GTIN = dto.GTIN,
            ProductName = dto.ProductName,
            Description = dto.Description,
            CustomerId = dto.CustomerId
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Yeni ürün oluşturuldu: {ProductId} - {ProductName}", product.Id, product.ProductName);

        var resultDto = new ProductDto(
            product.Id,
            product.GTIN,
            product.ProductName,
            product.Description,
            product.CustomerId,
            customer.CompanyName,
            product.CreatedAt,
            product.IsActive
        );

        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponse<ProductDto>.SuccessResponse(resultDto, "Ürün başarıyla oluşturuldu"));
    }

    
    /// Ürün günceller
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] ProductUpdateDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        
        if (product == null)
            return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Ürün bulunamadı. ID: {id}"));

        // GTIN validasyonu
        if (!_gs1Generator.ValidateGTIN(dto.GTIN))
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse($"Geçersiz GTIN formatı: {dto.GTIN}"));

        // GTIN benzersizlik kontrolü (kendisi hariç)
        if (await _unitOfWork.Products.GTINExistsAsync(dto.GTIN, id))
            return Conflict(ApiResponse<ProductDto>.ErrorResponse($"Bu GTIN zaten kayıtlı: {dto.GTIN}"));

        // Müşteri varlık kontrolü
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Müşteri bulunamadı. ID: {dto.CustomerId}"));

        product.GTIN = dto.GTIN;
        product.ProductName = dto.ProductName;
        product.Description = dto.Description;
        product.CustomerId = dto.CustomerId;
        product.IsActive = dto.IsActive;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ürün güncellendi: {ProductId}", id);

        var resultDto = new ProductDto(
            product.Id,
            product.GTIN,
            product.ProductName,
            product.Description,
            product.CustomerId,
            customer.CompanyName,
            product.CreatedAt,
            product.IsActive
        );

        return Ok(ApiResponse<ProductDto>.SuccessResponse(resultDto, "Ürün başarıyla güncellendi"));
    }

    
    /// Ürün siler (soft delete)
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        
        if (product == null)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Ürün bulunamadı. ID: {id}"));

        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ürün silindi: {ProductId}", id);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Ürün başarıyla silindi"));
    }
}

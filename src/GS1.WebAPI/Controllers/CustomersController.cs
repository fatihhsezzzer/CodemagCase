using GS1.Core.DTOs;
using GS1.Core.Entities;
using GS1.Core.Exceptions;
using GS1.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GS1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGS1GeneratorService _gs1Generator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        IUnitOfWork unitOfWork, 
        IGS1GeneratorService gs1Generator,
        ILogger<CustomersController> logger)
    {
        _unitOfWork = unitOfWork;
        _gs1Generator = gs1Generator;
        _logger = logger;
    }

    
    /// Tüm müşterileri listeler
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetAll()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        
        var dtos = customers.Select(c => new CustomerDto(
            c.Id,
            c.CompanyName,
            c.GLN,
            c.Description,
            c.CreatedAt,
            c.IsActive
        ));

        return Ok(ApiResponse<IEnumerable<CustomerDto>>.SuccessResponse(dtos));
    }

    
    /// ID'ye göre müşteri getirir
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        
        if (customer == null)
            return NotFound(ApiResponse<CustomerDto>.ErrorResponse($"Müşteri bulunamadı. ID: {id}"));

        var dto = new CustomerDto(
            customer.Id,
            customer.CompanyName,
            customer.GLN,
            customer.Description,
            customer.CreatedAt,
            customer.IsActive
        );

        return Ok(ApiResponse<CustomerDto>.SuccessResponse(dto));
    }

    
    /// Müşteri ve ürünlerini getirir
    
    [HttpGet("{id}/with-products")]
    public async Task<ActionResult<ApiResponse<CustomerWithProductsDto>>> GetWithProducts(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetWithProductsAsync(id);
        
        if (customer == null)
            return NotFound(ApiResponse<CustomerWithProductsDto>.ErrorResponse($"Müşteri bulunamadı. ID: {id}"));

        var dto = new CustomerWithProductsDto(
            customer.Id,
            customer.CompanyName,
            customer.GLN,
            customer.Description,
            customer.CreatedAt,
            customer.IsActive,
            customer.Products.Select(p => new ProductDto(
                p.Id,
                p.GTIN,
                p.ProductName,
                p.Description,
                p.CustomerId,
                customer.CompanyName,
                p.CreatedAt,
                p.IsActive
            )).ToList()
        );

        return Ok(ApiResponse<CustomerWithProductsDto>.SuccessResponse(dto));
    }

    
    /// Yeni müşteri oluşturur
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create([FromBody] CustomerCreateDto dto)
    {
        // GLN validasyonu
        if (!_gs1Generator.ValidateGLN(dto.GLN))
            return BadRequest(ApiResponse<CustomerDto>.ErrorResponse($"Geçersiz GLN formatı: {dto.GLN}"));

        // GLN benzersizlik kontrolü
        if (await _unitOfWork.Customers.GLNExistsAsync(dto.GLN))
            return Conflict(ApiResponse<CustomerDto>.ErrorResponse($"Bu GLN zaten kayıtlı: {dto.GLN}"));

        var customer = new Customer
        {
            CompanyName = dto.CompanyName,
            GLN = dto.GLN,
            Description = dto.Description
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Yeni müşteri oluşturuldu: {CustomerId} - {CompanyName}", customer.Id, customer.CompanyName);

        var resultDto = new CustomerDto(
            customer.Id,
            customer.CompanyName,
            customer.GLN,
            customer.Description,
            customer.CreatedAt,
            customer.IsActive
        );

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, 
            ApiResponse<CustomerDto>.SuccessResponse(resultDto, "Müşteri başarıyla oluşturuldu"));
    }

    
    /// Müşteri günceller
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(Guid id, [FromBody] CustomerUpdateDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        
        if (customer == null)
            return NotFound(ApiResponse<CustomerDto>.ErrorResponse($"Müşteri bulunamadı. ID: {id}"));

        // GLN validasyonu
        if (!_gs1Generator.ValidateGLN(dto.GLN))
            return BadRequest(ApiResponse<CustomerDto>.ErrorResponse($"Geçersiz GLN formatı: {dto.GLN}"));

        // GLN benzersizlik kontrolü (kendisi hariç)
        if (await _unitOfWork.Customers.GLNExistsAsync(dto.GLN, id))
            return Conflict(ApiResponse<CustomerDto>.ErrorResponse($"Bu GLN zaten kayıtlı: {dto.GLN}"));

        customer.CompanyName = dto.CompanyName;
        customer.GLN = dto.GLN;
        customer.Description = dto.Description;
        customer.IsActive = dto.IsActive;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Müşteri güncellendi: {CustomerId}", id);

        var resultDto = new CustomerDto(
            customer.Id,
            customer.CompanyName,
            customer.GLN,
            customer.Description,
            customer.CreatedAt,
            customer.IsActive
        );

        return Ok(ApiResponse<CustomerDto>.SuccessResponse(resultDto, "Müşteri başarıyla güncellendi"));
    }

    
    /// Müşteri siler (soft delete)
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        
        if (customer == null)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Müşteri bulunamadı. ID: {id}"));

        await _unitOfWork.Customers.DeleteAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Müşteri silindi: {CustomerId}", id);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Müşteri başarıyla silindi"));
    }
}

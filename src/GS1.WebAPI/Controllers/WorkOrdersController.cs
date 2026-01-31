using GS1.Core.DTOs;
using GS1.Core.Entities;
using GS1.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GS1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISerializationService _serializationService;
    private readonly ILogger<WorkOrdersController> _logger;

    public WorkOrdersController(
        IUnitOfWork unitOfWork,
        ISerializationService serializationService,
        ILogger<WorkOrdersController> logger)
    {
        _unitOfWork = unitOfWork;
        _serializationService = serializationService;
        _logger = logger;
    }

    
    /// Tüm iş emirlerini listeler
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderDto>>>> GetAll()
    {
        var workOrders = await _unitOfWork.WorkOrders.GetAllAsync();
        
        var dtos = workOrders.Select(w => MapToDto(w));

        return Ok(ApiResponse<IEnumerable<WorkOrderDto>>.SuccessResponse(dtos));
    }

    
    /// ID'ye göre iş emri getirir
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WorkOrderDto>>> GetById(Guid id)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetWithDetailsAsync(id);
        
        if (workOrder == null)
            return NotFound(ApiResponse<WorkOrderDto>.ErrorResponse($"İş emri bulunamadı. ID: {id}"));

        return Ok(ApiResponse<WorkOrderDto>.SuccessResponse(MapToDto(workOrder)));
    }

    
    /// İş emri detayı - Tüm bilgilerle birlikte
    /// İş emri, ürün, müşteri, seri numaraları ve agregasyon yapısı
    
    [HttpGet("{id}/details")]
    public async Task<ActionResult<ApiResponse<WorkOrderDetailDto>>> GetDetails(Guid id)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetFullDetailsAsync(id);
        
        if (workOrder == null)
            return NotFound(ApiResponse<WorkOrderDetailDto>.ErrorResponse($"İş emri bulunamadı. ID: {id}"));

        var product = workOrder.Product;
        var customer = product.Customer;

        // Seri numaralarını DTO'ya dönüştür
        var serialNumbers = workOrder.SerialNumbers.Select(s => new SerialNumberDto(
            s.Id,
            s.Serial,
            s.GS1DataMatrix,
            s.Status,
            s.Status.ToString(),
            s.PrintedAt,
            s.VerifiedAt,
            s.WorkOrderId,
            s.SSCCId,
            s.SSCC?.SSCCCode,
            s.CreatedAt
        )).ToList();

        // Agregasyon yapısını oluştur (Paletler ve Koliler)
        var aggregations = workOrder.SSCCs
            .Where(s => s.Type == SSCCType.Pallet || (s.Type == SSCCType.Box && s.ParentSSCCId == null))
            .Select(s => MapToAggregationDto(s, workOrder.SSCCs.ToList(), workOrder.SerialNumbers.ToList()))
            .ToList();

        // Özet bilgiler
        var summary = new WorkOrderSummaryDto(
            TotalSerialNumbers: workOrder.SerialNumbers.Count,
            PrintedCount: workOrder.SerialNumbers.Count(s => s.Status >= SerialNumberStatus.Printed),
            VerifiedCount: workOrder.SerialNumbers.Count(s => s.Status == SerialNumberStatus.Verified),
            RejectedCount: workOrder.SerialNumbers.Count(s => s.Status == SerialNumberStatus.Rejected),
            AggregatedCount: workOrder.SerialNumbers.Count(s => s.Status == SerialNumberStatus.Aggregated),
            TotalBoxes: workOrder.SSCCs.Count(s => s.Type == SSCCType.Box),
            TotalPallets: workOrder.SSCCs.Count(s => s.Type == SSCCType.Pallet),
            CompletionPercentage: workOrder.ProductionQuantity > 0 
                ? Math.Round((double)workOrder.SerialNumbers.Count / workOrder.ProductionQuantity * 100, 2) 
                : 0
        );

        var dto = new WorkOrderDetailDto(
            Id: workOrder.Id,
            WorkOrderNumber: workOrder.WorkOrderNumber,
            ProductionQuantity: workOrder.ProductionQuantity,
            BatchNumber: workOrder.BatchNumber,
            ExpirationDate: workOrder.ExpirationDate,
            SerialNumberStart: workOrder.SerialNumberStart,
            LastSerialNumber: workOrder.LastSerialNumber,
            Status: workOrder.Status,
            StatusText: workOrder.Status.ToString(),
            ItemsPerBox: workOrder.ItemsPerBox,
            BoxesPerPallet: workOrder.BoxesPerPallet,
            CreatedAt: workOrder.CreatedAt,
            Product: new ProductDto(
                product.Id,
                product.GTIN,
                product.ProductName,
                product.Description,
                product.CustomerId,
                customer.CompanyName,
                product.CreatedAt,
                product.IsActive
            ),
            Customer: new CustomerDto(
                customer.Id,
                customer.CompanyName,
                customer.GLN,
                customer.Description,
                customer.CreatedAt,
                customer.IsActive
            ),
            SerialNumbers: serialNumbers,
            Aggregations: aggregations,
            Summary: summary
        );

        return Ok(ApiResponse<WorkOrderDetailDto>.SuccessResponse(dto));
    }

    
    /// Duruma göre iş emirlerini listeler
    
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderDto>>>> GetByStatus(WorkOrderStatus status)
    {
        var workOrders = await _unitOfWork.WorkOrders.GetByStatusAsync(status);
        
        var dtos = workOrders.Select(w => MapToDto(w));

        return Ok(ApiResponse<IEnumerable<WorkOrderDto>>.SuccessResponse(dtos));
    }

    
    /// Ürüne göre iş emirlerini listeler
    
    [HttpGet("by-product/{productId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkOrderDto>>>> GetByProduct(Guid productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        
        if (product == null)
            return NotFound(ApiResponse<IEnumerable<WorkOrderDto>>.ErrorResponse($"Ürün bulunamadı. ID: {productId}"));

        var workOrders = await _unitOfWork.WorkOrders.GetByProductIdAsync(productId);
        
        var dtos = workOrders.Select(w => MapToDto(w, product));

        return Ok(ApiResponse<IEnumerable<WorkOrderDto>>.SuccessResponse(dtos));
    }

    
    /// Yeni iş emri oluşturur
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkOrderDto>>> Create([FromBody] WorkOrderCreateDto dto)
    {
        // Ürün varlık kontrolü
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            return NotFound(ApiResponse<WorkOrderDto>.ErrorResponse($"Ürün bulunamadı. ID: {dto.ProductId}"));

        // İş emri numarası benzersizlik kontrolü
        if (await _unitOfWork.WorkOrders.OrderNumberExistsAsync(dto.WorkOrderNumber))
            return Conflict(ApiResponse<WorkOrderDto>.ErrorResponse($"Bu iş emri numarası zaten kayıtlı: {dto.WorkOrderNumber}"));

        var workOrder = new WorkOrder
        {
            WorkOrderNumber = dto.WorkOrderNumber,
            ProductionQuantity = dto.ProductionQuantity,
            BatchNumber = dto.BatchNumber,
            ExpirationDate = dto.ExpirationDate,
            SerialNumberStart = dto.SerialNumberStart,
            ProductId = dto.ProductId,
            ItemsPerBox = dto.ItemsPerBox,
            BoxesPerPallet = dto.BoxesPerPallet,
            Status = WorkOrderStatus.Created
        };

        await _unitOfWork.WorkOrders.AddAsync(workOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Yeni iş emri oluşturuldu: {WorkOrderId} - {WorkOrderNumber}", workOrder.Id, workOrder.WorkOrderNumber);

        return CreatedAtAction(nameof(GetById), new { id = workOrder.Id },
            ApiResponse<WorkOrderDto>.SuccessResponse(MapToDto(workOrder, product), "İş emri başarıyla oluşturuldu"));
    }

    
    /// İş emri günceller
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WorkOrderDto>>> Update(Guid id, [FromBody] WorkOrderUpdateDto dto)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetWithDetailsAsync(id);
        
        if (workOrder == null)
            return NotFound(ApiResponse<WorkOrderDto>.ErrorResponse($"İş emri bulunamadı. ID: {id}"));

        // İş emri numarası benzersizlik kontrolü (kendisi hariç)
        if (await _unitOfWork.WorkOrders.OrderNumberExistsAsync(dto.WorkOrderNumber, id))
            return Conflict(ApiResponse<WorkOrderDto>.ErrorResponse($"Bu iş emri numarası zaten kayıtlı: {dto.WorkOrderNumber}"));

        workOrder.WorkOrderNumber = dto.WorkOrderNumber;
        workOrder.ProductionQuantity = dto.ProductionQuantity;
        workOrder.BatchNumber = dto.BatchNumber;
        workOrder.ExpirationDate = dto.ExpirationDate;
        workOrder.Status = dto.Status;
        workOrder.ItemsPerBox = dto.ItemsPerBox;
        workOrder.BoxesPerPallet = dto.BoxesPerPallet;
        workOrder.IsActive = dto.IsActive;

        await _unitOfWork.WorkOrders.UpdateAsync(workOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("İş emri güncellendi: {WorkOrderId}", id);

        return Ok(ApiResponse<WorkOrderDto>.SuccessResponse(MapToDto(workOrder), "İş emri başarıyla güncellendi"));
    }

    
    /// İş emri durumunu günceller
    
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<WorkOrderDto>>> UpdateStatus(Guid id, [FromBody] WorkOrderStatus newStatus)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetWithDetailsAsync(id);
        
        if (workOrder == null)
            return NotFound(ApiResponse<WorkOrderDto>.ErrorResponse($"İş emri bulunamadı. ID: {id}"));

        workOrder.Status = newStatus;

        await _unitOfWork.WorkOrders.UpdateAsync(workOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("İş emri durumu güncellendi: {WorkOrderId} - {Status}", id, newStatus);

        return Ok(ApiResponse<WorkOrderDto>.SuccessResponse(MapToDto(workOrder), "İş emri durumu güncellendi"));
    }

    
    /// İş emri için seri numaraları üretir
    
    [HttpPost("{id}/generate-serials")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SerialNumberDto>>>> GenerateSerials(Guid id, [FromBody] int quantity)
    {
        try
        {
            var serialNumbers = await _serializationService.GenerateSerialNumbersAsync(id, quantity);
            
            var dtos = serialNumbers.Select(s => new SerialNumberDto(
                s.Id,
                s.Serial,
                s.GS1DataMatrix,
                s.Status,
                s.Status.ToString(),
                s.PrintedAt,
                s.VerifiedAt,
                s.WorkOrderId,
                s.SSCCId,
                null,
                s.CreatedAt
            ));

            _logger.LogInformation("Seri numaraları üretildi: {WorkOrderId} - Adet: {Quantity}", id, quantity);

            return Ok(ApiResponse<IEnumerable<SerialNumberDto>>.SuccessResponse(dtos, $"{quantity} adet seri numarası üretildi"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<SerialNumberDto>>.ErrorResponse(ex.Message));
        }
        catch (Core.Exceptions.ValidationException ex)
        {
            return BadRequest(ApiResponse<IEnumerable<SerialNumberDto>>.ErrorResponse(ex.Message));
        }
        catch (Core.Exceptions.WorkOrderStatusException ex)
        {
            return BadRequest(ApiResponse<IEnumerable<SerialNumberDto>>.ErrorResponse(ex.Message));
        }
    }

    
    /// İş emri siler (soft delete)
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(id);
        
        if (workOrder == null)
            return NotFound(ApiResponse<bool>.ErrorResponse($"İş emri bulunamadı. ID: {id}"));

        await _unitOfWork.WorkOrders.DeleteAsync(workOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("İş emri silindi: {WorkOrderId}", id);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "İş emri başarıyla silindi"));
    }

    #region Private Methods

    private static WorkOrderDto MapToDto(WorkOrder w, Product? product = null)
    {
        var p = product ?? w.Product;
        return new WorkOrderDto(
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
            p?.ProductName,
            p?.GTIN,
            w.ItemsPerBox,
            w.BoxesPerPallet,
            w.CreatedAt,
            w.IsActive
        );
    }

    private static SSCCAggregationDto MapToAggregationDto(SSCC sscc, List<SSCC> allSSCCs, List<SerialNumber> allSerials)
    {
        List<SerialNumberDto>? items = null;
        List<SSCCAggregationDto>? childBoxes = null;

        if (sscc.Type == SSCCType.Box)
        {
            // Koli için içindeki ürünleri getir
            items = allSerials
                .Where(s => s.SSCCId == sscc.Id)
                .Select(s => new SerialNumberDto(
                    s.Id,
                    s.Serial,
                    s.GS1DataMatrix,
                    s.Status,
                    s.Status.ToString(),
                    s.PrintedAt,
                    s.VerifiedAt,
                    s.WorkOrderId,
                    s.SSCCId,
                    sscc.SSCCCode,
                    s.CreatedAt
                )).ToList();
        }
        else if (sscc.Type == SSCCType.Pallet)
        {
            // Palet için içindeki kolileri getir
            childBoxes = allSSCCs
                .Where(s => s.ParentSSCCId == sscc.Id && s.Type == SSCCType.Box)
                .Select(box => MapToAggregationDto(box, allSSCCs, allSerials))
                .ToList();
        }

        return new SSCCAggregationDto(
            sscc.Id,
            sscc.SSCCCode,
            sscc.Type,
            sscc.Type.ToString(),
            sscc.GS1DataMatrix,
            sscc.ParentSSCCId,
            items,
            childBoxes
        );
    }

    #endregion
}

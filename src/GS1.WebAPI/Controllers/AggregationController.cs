using GS1.Core.DTOs;
using GS1.Core.Entities;
using GS1.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GS1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AggregationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAggregationService _aggregationService;
    private readonly ILogger<AggregationController> _logger;

    public AggregationController(
        IUnitOfWork unitOfWork,
        IAggregationService aggregationService,
        ILogger<AggregationController> logger)
    {
        _unitOfWork = unitOfWork;
        _aggregationService = aggregationService;
        _logger = logger;
    }

    
    /// İş emrine göre SSCC'leri listeler
    
    [HttpGet("by-workorder/{workOrderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SSCCDto>>>> GetByWorkOrder(Guid workOrderId)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(workOrderId);
        
        if (workOrder == null)
            return NotFound(ApiResponse<IEnumerable<SSCCDto>>.ErrorResponse($"İş emri bulunamadı. ID: {workOrderId}"));

        var ssccs = await _unitOfWork.SSCCs.GetByWorkOrderIdAsync(workOrderId);
        
        var dtos = ssccs.Select(s => MapToDto(s));

        return Ok(ApiResponse<IEnumerable<SSCCDto>>.SuccessResponse(dtos));
    }

    
    /// İş emrine göre kolileri listeler (sadece atanmamış koliler)
    
    [HttpGet("boxes/{workOrderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SSCCDto>>>> GetBoxes(Guid workOrderId)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(workOrderId);
        
        if (workOrder == null)
            return NotFound(ApiResponse<IEnumerable<SSCCDto>>.ErrorResponse($"İş emri bulunamadı. ID: {workOrderId}"));

        var boxes = await _unitOfWork.SSCCs.GetByTypeAsync(SSCCType.Box, workOrderId);
        
        // Sadece palete atanmamış koliler için
        var unassignedBoxes = boxes.Where(b => b.ParentSSCCId == null);
        
        var dtos = unassignedBoxes.Select(s => MapToDto(s));

        return Ok(ApiResponse<IEnumerable<SSCCDto>>.SuccessResponse(dtos));
    }

    
    /// İş emrine göre paletleri listeler
    
    [HttpGet("pallets/{workOrderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SSCCDto>>>> GetPallets(Guid workOrderId)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(workOrderId);
        
        if (workOrder == null)
            return NotFound(ApiResponse<IEnumerable<SSCCDto>>.ErrorResponse($"İş emri bulunamadı. ID: {workOrderId}"));

        var pallets = await _unitOfWork.SSCCs.GetByTypeAsync(SSCCType.Pallet, workOrderId);
        
        var dtos = pallets.Select(s => MapToDto(s));

        return Ok(ApiResponse<IEnumerable<SSCCDto>>.SuccessResponse(dtos));
    }

    
    /// SSCC detayı ve agregasyon yapısını getirir
    
    [HttpGet("{id}/hierarchy")]
    public async Task<ActionResult<ApiResponse<SSCCAggregationDto>>> GetHierarchy(Guid id)
    {
        var sscc = await _aggregationService.GetAggregationHierarchyAsync(id);
        
        if (sscc == null)
            return NotFound(ApiResponse<SSCCAggregationDto>.ErrorResponse($"SSCC bulunamadı. ID: {id}"));

        var dto = MapToAggregationDto(sscc);

        return Ok(ApiResponse<SSCCAggregationDto>.SuccessResponse(dto));
    }

    
    /// Yeni koli oluşturur
    
    [HttpPost("create-box")]
    public async Task<ActionResult<ApiResponse<SSCCDto>>> CreateBox([FromBody] SSCCCreateDto dto)
    {
        try
        {
            var sscc = await _aggregationService.CreateBoxAsync(dto.WorkOrderId);
            
            _logger.LogInformation("Yeni koli oluşturuldu: {SSCCCode} - WorkOrder: {WorkOrderId}", sscc.SSCCCode, dto.WorkOrderId);

            return CreatedAtAction(nameof(GetHierarchy), new { id = sscc.Id },
                ApiResponse<SSCCDto>.SuccessResponse(MapToDto(sscc), "Koli başarıyla oluşturuldu"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SSCCDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Yeni palet oluşturur
    
    [HttpPost("create-pallet")]
    public async Task<ActionResult<ApiResponse<SSCCDto>>> CreatePallet([FromBody] SSCCCreateDto dto)
    {
        try
        {
            var sscc = await _aggregationService.CreatePalletAsync(dto.WorkOrderId);
            
            _logger.LogInformation("Yeni palet oluşturuldu: {SSCCCode} - WorkOrder: {WorkOrderId}", sscc.SSCCCode, dto.WorkOrderId);

            return CreatedAtAction(nameof(GetHierarchy), new { id = sscc.Id },
                ApiResponse<SSCCDto>.SuccessResponse(MapToDto(sscc), "Palet başarıyla oluşturuldu"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SSCCDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Ürünleri koliye ekler
    
    [HttpPost("add-items-to-box")]
    public async Task<ActionResult<ApiResponse<SSCCAggregationDto>>> AddItemsToBox([FromBody] AddItemsToBoxDto dto)
    {
        try
        {
            await _aggregationService.AddItemsToBoxAsync(dto.BoxId, dto.SerialNumberIds);
            
            var sscc = await _aggregationService.GetAggregationHierarchyAsync(dto.BoxId);
            
            _logger.LogInformation("Ürünler koliye eklendi: {BoxId} - Adet: {Count}", dto.BoxId, dto.SerialNumberIds.Count);

            return Ok(ApiResponse<SSCCAggregationDto>.SuccessResponse(
                MapToAggregationDto(sscc!), 
                $"{dto.SerialNumberIds.Count} ürün koliye eklendi"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SSCCAggregationDto>.ErrorResponse(ex.Message));
        }
        catch (Core.Exceptions.ValidationException ex)
        {
            return BadRequest(ApiResponse<SSCCAggregationDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Kolileri palete ekler
    
    [HttpPost("add-boxes-to-pallet")]
    public async Task<ActionResult<ApiResponse<SSCCAggregationDto>>> AddBoxesToPallet([FromBody] AddBoxesToPalletDto dto)
    {
        try
        {
            await _aggregationService.AddBoxesToPalletAsync(dto.PalletId, dto.BoxIds);
            
            var sscc = await _aggregationService.GetAggregationHierarchyAsync(dto.PalletId);
            
            _logger.LogInformation("Koliler palete eklendi: {PalletId} - Adet: {Count}", dto.PalletId, dto.BoxIds.Count);

            return Ok(ApiResponse<SSCCAggregationDto>.SuccessResponse(
                MapToAggregationDto(sscc!), 
                $"{dto.BoxIds.Count} koli palete eklendi"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SSCCAggregationDto>.ErrorResponse(ex.Message));
        }
        catch (Core.Exceptions.ValidationException ex)
        {
            return BadRequest(ApiResponse<SSCCAggregationDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Ürünü koliden çıkarır
    
    [HttpDelete("remove-item/{serialNumberId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveItemFromBox(Guid serialNumberId)
    {
        try
        {
            await _aggregationService.RemoveItemFromBoxAsync(serialNumberId);
            
            _logger.LogInformation("Ürün koliden çıkarıldı: {SerialNumberId}", serialNumberId);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Ürün koliden çıkarıldı"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    
    /// Koliyi paletten çıkarır
    
    [HttpDelete("remove-box/{boxId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveBoxFromPallet(Guid boxId)
    {
        try
        {
            await _aggregationService.RemoveBoxFromPalletAsync(boxId);
            
            _logger.LogInformation("Koli paletten çıkarıldı: {BoxId}", boxId);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Koli paletten çıkarıldı"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Core.Exceptions.ValidationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    #region Private Methods

    private static SSCCDto MapToDto(SSCC s)
    {
        // ItemCount: Palet için kolileri, koli için seri numaralarını say
        int itemCount = s.Type == SSCCType.Pallet 
            ? s.ChildSSCCs?.Count ?? 0 
            : s.SerialNumbers?.Count ?? 0;
            
        return new SSCCDto(
            s.Id,
            s.SSCCCode,
            s.Type,
            s.Type.ToString(),
            s.GS1DataMatrix,
            s.WorkOrderId,
            s.ParentSSCCId,
            s.ParentSSCC?.SSCCCode,
            itemCount,
            s.WorkOrder?.ItemsPerBox,
            s.CreatedAt
        );
    }

    private static SSCCAggregationDto MapToAggregationDto(SSCC sscc)
    {
        List<SerialNumberDto>? items = null;
        List<SSCCAggregationDto>? childBoxes = null;

        if (sscc.Type == SSCCType.Box && sscc.SerialNumbers != null)
        {
            items = sscc.SerialNumbers.Select(s => new SerialNumberDto(
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
        else if (sscc.Type == SSCCType.Pallet && sscc.ChildSSCCs != null)
        {
            childBoxes = sscc.ChildSSCCs.Select(MapToAggregationDto).ToList();
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

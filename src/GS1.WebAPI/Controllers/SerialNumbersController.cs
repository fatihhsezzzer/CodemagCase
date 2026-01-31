using GS1.Core.DTOs;
using GS1.Core.Entities;
using GS1.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GS1.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SerialNumbersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISerializationService _serializationService;
    private readonly ILogger<SerialNumbersController> _logger;

    public SerialNumbersController(
        IUnitOfWork unitOfWork,
        ISerializationService serializationService,
        ILogger<SerialNumbersController> logger)
    {
        _unitOfWork = unitOfWork;
        _serializationService = serializationService;
        _logger = logger;
    }

    
    /// İş emrine göre seri numaralarını listeler
    
    [HttpGet("by-workorder/{workOrderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SerialNumberDto>>>> GetByWorkOrder(Guid workOrderId)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(workOrderId);
        
        if (workOrder == null)
            return NotFound(ApiResponse<IEnumerable<SerialNumberDto>>.ErrorResponse($"İş emri bulunamadı. ID: {workOrderId}"));

        var serialNumbers = await _unitOfWork.SerialNumbers.GetByWorkOrderIdAsync(workOrderId);
        
        var dtos = serialNumbers.Select(s => MapToDto(s));

        return Ok(ApiResponse<IEnumerable<SerialNumberDto>>.SuccessResponse(dtos));
    }

    
    /// Atanmamış (koli dışı) seri numaralarını listeler
    
    [HttpGet("unassigned/{workOrderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SerialNumberDto>>>> GetUnassigned(Guid workOrderId)
    {
        var workOrder = await _unitOfWork.WorkOrders.GetByIdAsync(workOrderId);
        
        if (workOrder == null)
            return NotFound(ApiResponse<IEnumerable<SerialNumberDto>>.ErrorResponse($"İş emri bulunamadı. ID: {workOrderId}"));

        var serialNumbers = await _unitOfWork.SerialNumbers.GetUnassignedByWorkOrderAsync(workOrderId);
        
        var dtos = serialNumbers.Select(s => MapToDto(s));

        return Ok(ApiResponse<IEnumerable<SerialNumberDto>>.SuccessResponse(dtos));
    }

    
    /// ID'ye göre seri numarası getirir
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SerialNumberDto>>> GetById(Guid id)
    {
        var serialNumber = await _unitOfWork.SerialNumbers.GetByIdAsync(id);
        
        if (serialNumber == null)
            return NotFound(ApiResponse<SerialNumberDto>.ErrorResponse($"Seri numarası bulunamadı. ID: {id}"));

        return Ok(ApiResponse<SerialNumberDto>.SuccessResponse(MapToDto(serialNumber)));
    }

    
    /// Seri numarasını basıldı olarak işaretler
    
    [HttpPatch("{id}/mark-printed")]
    public async Task<ActionResult<ApiResponse<SerialNumberDto>>> MarkAsPrinted(Guid id)
    {
        try
        {
            await _serializationService.MarkAsPrintedAsync(id);
            
            var serialNumber = await _unitOfWork.SerialNumbers.GetByIdAsync(id);
            
            _logger.LogInformation("Seri numarası basıldı olarak işaretlendi: {SerialNumberId}", id);

            return Ok(ApiResponse<SerialNumberDto>.SuccessResponse(MapToDto(serialNumber!), "Seri numarası basıldı olarak işaretlendi"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SerialNumberDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Seri numarasını doğrulandı olarak işaretler
    
    [HttpPatch("{id}/mark-verified")]
    public async Task<ActionResult<ApiResponse<SerialNumberDto>>> MarkAsVerified(Guid id)
    {
        try
        {
            await _serializationService.MarkAsVerifiedAsync(id);
            
            var serialNumber = await _unitOfWork.SerialNumbers.GetByIdAsync(id);
            
            _logger.LogInformation("Seri numarası doğrulandı olarak işaretlendi: {SerialNumberId}", id);

            return Ok(ApiResponse<SerialNumberDto>.SuccessResponse(MapToDto(serialNumber!), "Seri numarası doğrulandı olarak işaretlendi"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SerialNumberDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Seri numarasını reddedildi olarak işaretler
    
    [HttpPatch("{id}/mark-rejected")]
    public async Task<ActionResult<ApiResponse<SerialNumberDto>>> MarkAsRejected(Guid id)
    {
        try
        {
            await _serializationService.MarkAsRejectedAsync(id);
            
            var serialNumber = await _unitOfWork.SerialNumbers.GetByIdAsync(id);
            
            _logger.LogInformation("Seri numarası reddedildi olarak işaretlendi: {SerialNumberId}", id);

            return Ok(ApiResponse<SerialNumberDto>.SuccessResponse(MapToDto(serialNumber!), "Seri numarası reddedildi olarak işaretlendi"));
        }
        catch (Core.Exceptions.NotFoundException ex)
        {
            return NotFound(ApiResponse<SerialNumberDto>.ErrorResponse(ex.Message));
        }
    }

    
    /// Toplu olarak basıldı işaretleme
    
    [HttpPatch("bulk-mark-printed")]
    public async Task<ActionResult<ApiResponse<int>>> BulkMarkAsPrinted([FromBody] List<Guid> ids)
    {
        var count = 0;
        foreach (var id in ids)
        {
            try
            {
                await _serializationService.MarkAsPrintedAsync(id);
                count++;
            }
            catch { /* Hatalı ID'leri atla */ }
        }

        _logger.LogInformation("Toplu basıldı işaretleme: {Count}/{Total}", count, ids.Count);

        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} adet seri numarası basıldı olarak işaretlendi"));
    }

    
    /// Toplu olarak doğrulandı işaretleme
    
    [HttpPatch("bulk-mark-verified")]
    public async Task<ActionResult<ApiResponse<int>>> BulkMarkAsVerified([FromBody] List<Guid> ids)
    {
        var count = 0;
        foreach (var id in ids)
        {
            try
            {
                await _serializationService.MarkAsVerifiedAsync(id);
                count++;
            }
            catch { /* Hatalı ID'leri atla */ }
        }

        _logger.LogInformation("Toplu doğrulandı işaretleme: {Count}/{Total}", count, ids.Count);

        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} adet seri numarası doğrulandı olarak işaretlendi"));
    }

    private static SerialNumberDto MapToDto(SerialNumber s)
    {
        return new SerialNumberDto(
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
        );
    }
}

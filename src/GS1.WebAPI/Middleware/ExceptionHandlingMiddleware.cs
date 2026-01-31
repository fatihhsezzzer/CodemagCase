using System.Net;
using System.Text.Json;
using GS1.Core.DTOs;
using GS1.Core.Exceptions;

namespace GS1.WebAPI.Middleware;


/// Global exception handling middleware

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException ex => (HttpStatusCode.NotFound, ex.Message, null as List<string>),
            ValidationException ex => (HttpStatusCode.BadRequest, ex.Message, ex.Errors),
            DuplicateException ex => (HttpStatusCode.Conflict, ex.Message, null),
            GS1ValidationException ex => (HttpStatusCode.BadRequest, ex.Message, null),
            WorkOrderStatusException ex => (HttpStatusCode.BadRequest, ex.Message, null),
            BusinessException ex => (HttpStatusCode.BadRequest, ex.Message, null),
            ArgumentNullException ex => (HttpStatusCode.BadRequest, $"Geçersiz parametre: {ex.ParamName}", null),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message, null),
            _ => (HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu", null)
        };

        response.StatusCode = (int)statusCode;

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Beklenmeyen hata: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("İş mantığı hatası: {StatusCode} - {Message}", statusCode, message);
        }

        var apiResponse = ApiResponse<object>.ErrorResponse(message, errors);
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(apiResponse, options));
    }
}


/// Extension method for adding exception handling middleware

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

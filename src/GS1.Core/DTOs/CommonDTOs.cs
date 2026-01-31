using System.Text.Json.Serialization;

namespace GS1.Core.DTOs;

// ================== Common DTOs ==================


/// API Response wrapper

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    public ApiResponse() { }

    public ApiResponse(bool success, string message, T? data, List<string>? errors = null)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "İşlem başarılı")
        => new(true, message, data);

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        => new(false, message, default, errors);
}


/// Sayfalama için request

public record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false
);


/// Sayfalanmış response

public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);

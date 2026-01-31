namespace GS1.Core.Exceptions;


/// Temel iş mantığı exception sınıfı

public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string message, string code = "BUSINESS_ERROR") 
        : base(message)
    {
        Code = code;
    }
}


/// Kayıt bulunamadı exception'ı

public class NotFoundException : BusinessException
{
    public NotFoundException(string entityName, object id) 
        : base($"{entityName} bulunamadı. ID: {id}", "NOT_FOUND")
    {
    }
}


/// Validasyon exception'ı

public class ValidationException : BusinessException
{
    public List<string> Errors { get; }

    public ValidationException(string message, List<string>? errors = null) 
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors ?? new List<string>();
    }
}


/// Duplicate kayıt exception'ı

public class DuplicateException : BusinessException
{
    public DuplicateException(string entityName, string fieldName, object value) 
        : base($"{entityName} zaten mevcut. {fieldName}: {value}", "DUPLICATE_ERROR")
    {
    }
}


/// GS1 formatı geçersiz exception'ı

public class GS1ValidationException : BusinessException
{
    public GS1ValidationException(string fieldName, string value) 
        : base($"Geçersiz GS1 formatı. {fieldName}: {value}", "GS1_VALIDATION_ERROR")
    {
    }
}


/// İş emri durumu exception'ı

public class WorkOrderStatusException : BusinessException
{
    public WorkOrderStatusException(string message) 
        : base(message, "WORKORDER_STATUS_ERROR")
    {
    }
}

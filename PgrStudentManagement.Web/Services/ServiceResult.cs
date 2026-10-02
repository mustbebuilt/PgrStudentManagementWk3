namespace PgrStudentManagement.Web.Services;

/// <summary>
/// Encapsulates the outcome of a business layer operation.
/// </summary>
public class ServiceResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    protected ServiceResult(bool success, string? errorMessage = null, string? errorCode = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string errorMessage, string? errorCode = null) => new(false, errorMessage, errorCode);
}

/// <summary>
/// Encapsulates the outcome of a business layer operation returning a value.
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    private ServiceResult(bool success, T? data, string? errorMessage = null, string? errorCode = null)
        : base(success, errorMessage, errorCode)
    {
        Data = data;
    }

    public static ServiceResult<T> Ok(T data) => new(true, data);
    public static new ServiceResult<T> Fail(string errorMessage, string? errorCode = null) => new(false, default, errorMessage, errorCode);
}

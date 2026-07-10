namespace PodcastPlatform.Services.Models;

public enum ServiceStatus
{
    Ok,
    Created,
    NoContent,
    NotFound,
    Forbidden,
    Unauthorized,
    BadRequest
}

public class ServiceResult
{
    public ServiceStatus Status { get; }
    public string? Message { get; }

    protected ServiceResult(ServiceStatus status, string? message = null)
    {
        Status = status;
        Message = message;
    }

    public static ServiceResult Ok() => new(ServiceStatus.Ok);
    public static ServiceResult NoContent() => new(ServiceStatus.NoContent);
    public static ServiceResult NotFound(string message) => new(ServiceStatus.NotFound, message);
    public static ServiceResult Forbidden(string? message = null) => new(ServiceStatus.Forbidden, message);
    public static ServiceResult Unauthorized(string? message = null) => new(ServiceStatus.Unauthorized, message);
    public static ServiceResult BadRequest(string message) => new(ServiceStatus.BadRequest, message);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }
    public string? ActionName { get; }
    public object? RouteValues { get; }

    private ServiceResult(ServiceStatus status, T? data = default, string? message = null, string? actionName = null, object? routeValues = null)
        : base(status, message)
    {
        Data = data;
        ActionName = actionName;
        RouteValues = routeValues;
    }

    public static ServiceResult<T> Ok(T data) => new(ServiceStatus.Ok, data);
    public static ServiceResult<T> Created(T data, string actionName, object? routeValues) => new(ServiceStatus.Created, data, null, actionName, routeValues);
    public new static ServiceResult<T> NoContent() => new(ServiceStatus.NoContent);
    public new static ServiceResult<T> NotFound(string message) => new(ServiceStatus.NotFound, default, message);
    public new static ServiceResult<T> Forbidden(string? message = null) => new(ServiceStatus.Forbidden, default, message);
    public new static ServiceResult<T> Unauthorized(string? message = null) => new(ServiceStatus.Unauthorized, default, message);
    public new static ServiceResult<T> BadRequest(string message) => new(ServiceStatus.BadRequest, default, message);
}

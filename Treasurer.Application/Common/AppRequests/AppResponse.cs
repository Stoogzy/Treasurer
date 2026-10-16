using System.Text.Json.Serialization;

namespace Treasurer.Application.Common.AppRequests;

public record AppResponse
{
    [JsonIgnore]
    public int StatusCode { get; init; }
    public string Message { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }

    public AppResponse(int statusCode, string message = "", IDictionary<string, string[]>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }

    public static AppResponse Ok() => new(200);
    public static AppResponse Created() => new(201);
    public static AppResponse NoContent() => new(204);
    public static AppResponse BadRequest(string message) => new(400, message);
    public static AppResponse ValidationFailed(IDictionary<string, string[]> errors) => new(400, "One or more validation errors occurred.") { Errors = errors };
    public static AppResponse Unauthorized() => new(401);
    public static AppResponse Forbidden() => new(403);
    public static AppResponse NotFound() => new(404);
    public static AppResponse Conflict(string message) => new(409, message);
}

public record AppResponse<T> : AppResponse
{
    public T? Content { get; init; }

    public AppResponse(int statusCode, T? content = default, string message = "", IDictionary<string, string[]>? errors = null)
        : base(statusCode, message, errors)
    {
        Content = content;
    }

    public static AppResponse<T> Ok(T content) => new(200, content);
    public static AppResponse<T> Created(T content) => new(201, content);
}

namespace RiskManagement.Application.Common;

public class ResultError
{
    public string Message { get; }
    public int StatusCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }
    public object? Values { get; }

    public ResultError(string message, int statusCode, Dictionary<string, string[]>? validationErrors = null,
        object? values = null)
    {
        Message = message;
        StatusCode = statusCode;
        ValidationErrors = validationErrors;
        Values = values;
    }
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ResultError? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(ResultError error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure(string message, int statusCode = 400)
    {
        return new Result<T>(new ResultError(message, statusCode));
    }

    public static Result<T> NotFound(string message = "Not found")
    {
        return new Result<T>(new ResultError(message, 404));
    }

    public static Result<T> Forbidden(string message = "Forbidden")
    {
        return new Result<T>(new ResultError(message, 403));
    }

    public static Result<T> ValidationFailure(Dictionary<string, string[]> errors, object? values = null)
    {
        return new Result<T>(new ResultError("Validation failed", 400, errors, values));
    }
}
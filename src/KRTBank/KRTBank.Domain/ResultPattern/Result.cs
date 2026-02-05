using Microsoft.AspNetCore.Http;

namespace KRTBank.Domain.ResultPattern;

public class Result<T> : Result
{
    public T? Data { get; private set; }

    protected Result(
        bool success,
        string? message = default,
        T? data = default,
        int? code = null)
        : base(success, message, code)
    {
        Data = data;
    }

    public static Result<T> Ok(
        string? message = default,
        T? data = default,
        int? code = StatusCodes.Status200OK)
    {
        return new Result<T>(true, message, data, code);
    }

    public static Result<T> Fail(
        string message,
        int? code = null,
        T? data = default)
    {
        return new Result<T>(false, message, data, code);
    }
}

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Message { get; protected set; }
    public int? Code { get; protected set; }

    protected Result(
        bool success,
        string? message = default,
        int? code = null)
    {
        IsSuccess = success;
        Message = message;
        Code = code;
    }

    public static Result Ok(
        string? message = default,
        int? code = StatusCodes.Status200OK)
    {
        return new Result(true, message, code);
    }

    public static Result Fail(
        string message,
        int? code = null)
    {
        return new Result(false, message, code);
    }
}
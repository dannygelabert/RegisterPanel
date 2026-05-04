namespace RegisterPanel.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    protected Result() { IsSuccess = true; }

    protected Result(string errorCode, string errorMessage)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new();
    public static Result Failure(string errorCode, string errorMessage) => new(errorCode, errorMessage);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) { Value = value; }
    private Result(string errorCode, string errorMessage) : base(errorCode, errorMessage) { }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string errorCode, string errorMessage) => new(errorCode, errorMessage);
}

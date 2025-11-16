namespace ScanVul.Agent.Installer;

using System.Text.Json.Serialization;

public class Result
{
    protected internal Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null ||
            !isSuccess && error == null)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    
    public bool IsFailure => !IsSuccess;
    
    public string? Error { get; }

    public static Result Success() => new(true, null);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(string error, params List<Exception> exceptions) => new(false, $"{error}\n\n{string.Join("\n\n", exceptions)}");
    public static Result<TValue> Failure<TValue>(string error) => new(default, false, error);
    public static Result<TValue> Failure<TValue>(string error, params List<Exception> exceptions) => new(default, false, $"{error}\n\n{string.Join("\n\n", exceptions)}");

}

public class Result<TValue> : Result
{
    private readonly TValue? _value;
    
    [JsonConstructor]
    protected internal Result(TValue? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No access to value when result is failure");
    
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>("Null value");
}
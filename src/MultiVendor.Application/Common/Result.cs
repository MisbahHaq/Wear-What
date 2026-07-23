namespace MultiVendor.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Error { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static Result<T> Success(T data)
    {
        return new Result<T> { IsSuccess = true, Data = data };
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T> { IsSuccess = false, Error = error };
    }

    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T> { IsSuccess = false, Errors = errors };
    }
}

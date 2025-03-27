namespace Dirassati_Backend.Common;

public class Result<TResult, TError>
{
    public bool IsSuccess { get; private set; }
    public TError? Errors { get; private set; }
    public TResult? Value { get; private set; }
    public int StatusCode { get; private set; }

    public Result<TResult, TError> Success(TResult value, int statusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(value);
        return
         new()
         {
             IsSuccess = true,
             Value = value,
             StatusCode = statusCode
         };

    }
    public Result<TResult, TError> Failure(TError errors, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new()
        {
            IsSuccess = false,
            Errors = errors,
            StatusCode = statusCode
        };
    }

    internal void Success()
    {
        throw new NotImplementedException();
    }
}
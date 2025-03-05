namespace Dirassati_Backend.Common;

public class Result<TResult, TError>
{
    public bool IsSuccess { get; private set; }
    public TError? Errors { get; private set; }
    public TResult? Value { get; private set; }
    public int? ErrorCode { get; private set; }

    public static Result<TResult, TError> Success(TResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return
         new()
         {
             IsSuccess = true,
             Value = value
         };

    }
    public static Result<TResult, TError> Failure(int errorCode, TError errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new()
        {
            IsSuccess = false,
            Errors = errors,
            ErrorCode = errorCode
        };
    }
}
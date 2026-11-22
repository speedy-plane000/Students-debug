
public class Result<T>
{
     public bool IsSuccess {get;}
    public T? Value {get;}
    public ErrorType ErrorType {get;}
    public string? TextError {get;}
    public Dictionary<string,string>? Errors {get;}

    public Result(T value)
    {
        IsSuccess = true;
        Value = value;
        ErrorType = ErrorType.None;
    }
    public Result(ErrorType errorType,string textError)
    {
        IsSuccess = false;
        ErrorType = errorType;
        TextError = textError;
    }
    public Result(T value,ErrorType errorType)
    {
        IsSuccess = false;
        Value = value;
        ErrorType = errorType;
    }
    public Result(ErrorType errorType, Dictionary<string, string> errors)
    {
        IsSuccess = false;
        ErrorType = errorType;
        Errors = errors;
    }
}
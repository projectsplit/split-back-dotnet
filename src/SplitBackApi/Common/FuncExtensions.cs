using CSharpFunctionalExtensions;

namespace SplitBackApi.Common;

public static class FuncExtensions
{
    public static Result<T> ExecuteResult<T>(this Func<T> func)
    {
        try
        {
            var value = func.Invoke();
            return Result.Success(value);
        }
        catch (Exception e)
        {
            return Result.Failure<T>(e.Message);
        }
    }
    
    public static Result ExecuteResult<T>(this Action action)
    {
        try
        {
            action.Invoke();
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure<T>(e.Message);
        }
    }
}
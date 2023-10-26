using CSharpFunctionalExtensions;

namespace SplitBackApi.Common;

public static class TaskExtensions
{
    public static async Task<Result<T>> ExecuteResultAsync<T>(this Task<T> task)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return Result.Success(result);
        }
        catch (Exception e)
        {
            return Result.Failure<T>(e.Message);
        }
    }

    public static async Task<Result> ExecuteResultAsync(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
}
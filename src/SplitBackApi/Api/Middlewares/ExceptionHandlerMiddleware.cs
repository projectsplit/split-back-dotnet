using MongoDB.Driver;

namespace SplitBackApi.Api.Middlewares;

public class ExceptionHandlerMiddleware : IMiddleware {

  public async Task InvokeAsync(HttpContext context, RequestDelegate next) {

    try {

      await next(context);

    } catch(BadHttpRequestException e) {

      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      context.Response.ContentType = "application/json";

      await context.Response.WriteAsJsonAsync(new {
        Error = e.GetType().Name,
        Message = e.ToString()
      });

    } catch(FormatException e) {

      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      context.Response.ContentType = "application/json";

      await context.Response.WriteAsJsonAsync(new {
        Error = e.GetType().Name,
        Message = e.Message
      });

    } catch(MongoException e) {

      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      context.Response.ContentType = "application/json";

      await context.Response.WriteAsJsonAsync(new {
        Error = e.GetType().Name,
        Message = e.ToString()
      });
    }
  }
}
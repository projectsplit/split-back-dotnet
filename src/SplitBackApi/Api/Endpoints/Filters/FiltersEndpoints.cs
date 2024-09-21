namespace SplitBackApi.Api.Endpoints.Filters;

public static partial class FiltersEndpoints
{
  public static void MapFiltersEndpoints(this IEndpointRouteBuilder app)
  {
    var filtersGroup = app
        .MapGroup("/filters")
        .WithTags("Filters")
        .AllowAnonymous();

    filtersGroup.MapPost("/create", CreateFilters);
    //filtersGroup.MapGet("/getfilters",GetGroupFilters);

  }
}
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SplitBackApi.Api.Endpoints.Budgets.Responses;

namespace SplitBackApi.Api.Services.HttpClients;

public class ExchangeRateClient
{
  private readonly HttpClient _httpClient;

  public ExchangeRateClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
    _httpClient.BaseAddress = new Uri("https://openexchangerates.org/api/");
  }

  public async Task<Result<ExchangeRateResponse>> GetHistorical(string symbols, string baseCurrency, string date)
  {
    var requestUri = $"historical/{date}.json";

    var query = new Dictionary<string, string>
      {
        { "app_id", "382c2dbb473546f2aa9f558a18c8da29" },
        { "base", baseCurrency },
        { "symbols", symbols },
        { "show_alternative", "false" },
        { "prettyprint", "false" },
        { "date", date }
      };

    var fullUri = QueryHelpers.AddQueryString(requestUri, query);
    var response = await _httpClient.GetAsync(fullUri);

    string responseString = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
      // Handle error if needed
      return Result.Failure<ExchangeRateResponse>("Failed to retrieve exchange rates");
    }

    var deserializedResponse = JsonConvert.DeserializeObject<ExchangeRateResponse>(responseString);
    return Result.Success(deserializedResponse);
  }
}

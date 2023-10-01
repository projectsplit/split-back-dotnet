using CSharpFunctionalExtensions;
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
    var uriBuilder = new UriBuilder($"historical/{date}.json");

    var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
    query["app_id"] = "382c2dbb473546f2aa9f558a18c8da29";
    query["base"] = baseCurrency;
    query["symbols"] = symbols;
    query["show_alternative"] = "false";
    query["prettyprint"] = "false";
    query["date"] = date;

    uriBuilder.Query = query.ToString();
    string url = uriBuilder.ToString();

    var response = await _httpClient.GetAsync(url);
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

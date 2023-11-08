using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Services.HttpClients;

public class ExchangeRateClient
{
  private readonly HttpClient _httpClient;
  private readonly string _openExchangeRatesAppId;

  public ExchangeRateClient(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
  {
    _httpClient = httpClientFactory.CreateClient("openexchangerates");
    _openExchangeRatesAppId = appSettings.Value.OpenExchangeRatesAppId;
  }

  public async Task<Result<ExchangeRates>> GetHistorical(string symbols, string baseCurrency, string date)
  {
    var requestUri = $"historical/{date}.json";

    var query = new Dictionary<string, string>
      {
        { "app_id", _openExchangeRatesAppId },
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
      return Result.Failure<ExchangeRates>("Failed to retrieve exchange rates");
    }
    
    var deserializedResponse = JsonConvert.DeserializeObject<ExchangeRates>(responseString);
    
    deserializedResponse.Date = date;
    deserializedResponse.CreationTime = DateTime.UtcNow;
    deserializedResponse.LastUpdateTime = DateTime.UtcNow;;
      
    return Result.Success(deserializedResponse);
  }
}

using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExchangeRateRepository;

public class ExchangeRateRepository : IExchangeRateRepository
{
  private readonly ExchangeRateService _exchangeRateService;
  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<ExchangeRates> _exchangeRatesCollection;

  public ExchangeRateRepository(ExchangeRateService exchangeRateService, IOptions<AppSettings> appSettings)
  {
    _exchangeRateService = exchangeRateService;

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _exchangeRatesCollection = mongoDatabase.GetCollection<ExchangeRates>(dbSettings.Database.Collections.ExchangeRates);

  }

  public async Task<Result> GetExchangeRatesFromExternalProvider(string baseCurrency, string date)
  {
    var exchangeRatesResult = await _exchangeRateService.HistoricalFxRates(baseCurrency, date);
    if (exchangeRatesResult.IsFailure) return Result.Failure<ExchangeRates>(exchangeRatesResult.Error);

    var exchangeRates = exchangeRatesResult.Value;
    await _exchangeRatesCollection.InsertOneAsync(exchangeRates);

    return Result.Success();
  }

  public async Task<Result<decimal>> GetRate(string fromCurrency, string toCurrency, string date)
  {
    var ratesByDateResult = await GetExchangeRatesByDate(date);
    if (ratesByDateResult.IsFailure) return Result.Failure<decimal>($"Could not retrieve rates for {date}");
    var ratesByDate = ratesByDateResult.Value;

    if (!ratesByDate.Rates.ContainsKey(fromCurrency) ||
        !ratesByDate.Rates.ContainsKey(toCurrency))
      return Result.Failure<decimal>($" {fromCurrency}/{toCurrency} rate does not exist on {date} or in DB");

    switch (fromCurrency)
    {
      case "USD":
        var rate = ratesByDate.Rates[toCurrency];
        return Result.Success(rate);

      default:
        var denominatorRate = ratesByDate.Rates[fromCurrency];
        var nominatorRate = ratesByDate.Rates[toCurrency];
        rate = nominatorRate / denominatorRate;
        return Result.Success(rate);
    }
  }

  private async Task<Result<ExchangeRates>> GetExchangeRatesByDate(string date)
  {
    var filter = Builders<ExchangeRates>.Filter.Eq(e => e.Date, date);

    var exchangeRates = await _exchangeRatesCollection.Find(filter).FirstOrDefaultAsync();

    return exchangeRates ?? Result.Failure<ExchangeRates>($"Exchange rates for {date} not found");
  }

}
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExchangeRateRepository;

public class ExchangeRateMongoDbRepository : IExchangeRateRepository
{
  private readonly ExchangeRateService _exchangeRateService;
  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<ExchangeRates> _exchangeRatesCollection;

  public ExchangeRateMongoDbRepository(ExchangeRateService exchangeRateService, IOptions<AppSettings> appSettings)
  {
    _exchangeRateService = exchangeRateService;

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _exchangeRatesCollection = mongoDatabase.GetCollection<ExchangeRates>(dbSettings.Database.Collections.ExchangeRates);

  }

  public async Task<Maybe<List<ExchangeRates>>> GetAllRatesForDates(List<string> dates)
  {
    var filter = Builders<ExchangeRates>.Filter.In("Date", dates);
    var exchangeRates = await _exchangeRatesCollection.Find(filter).ToListAsync();

    return exchangeRates;
  }

  public async Task<Result> GetExchangeRatesFromExternalProvider(string baseCurrency, string date)
  {
    var rateExists = await _exchangeRatesCollection.Find(r => r.Date == date).AnyAsync(); //use timestamp or creationTime to see if it was fetched at the end of the day or on the go by the user.
    if (rateExists) return Result.Success();

    var exchangeRatesResult = await _exchangeRateService.HistoricalFxRates(baseCurrency, date);
    if (exchangeRatesResult.IsFailure) return Result.Failure<ExchangeRates>(exchangeRatesResult.Error);

    var exchangeRates = exchangeRatesResult.Value;
    await _exchangeRatesCollection.InsertOneAsync(exchangeRates);

    return Result.Success();
  }

  public async Task<Result<decimal>> GetRate(string fromCurrency, string toCurrency, string date) //Maybe
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

  public async Task<Result<ExchangeRates>> GetExchangeRatesByDate(string date)
  {
    var filter = Builders<ExchangeRates>.Filter.Eq(e => e.Date, date);

    var exchangeRates = await _exchangeRatesCollection.Find(filter).FirstOrDefaultAsync();

    return exchangeRates ?? Result.Failure<ExchangeRates>($"Exchange rates for {date} not found");
  }

}
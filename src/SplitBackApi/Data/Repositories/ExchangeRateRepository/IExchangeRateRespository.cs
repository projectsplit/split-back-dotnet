using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExchangeRateRepository;

public interface IExchangeRateRepository
{
  Task<Result> GetExchangeRatesFromExternalProvider(string baseCurrency, string date);

  Task<Maybe<List<ExchangeRates>>> GetAllRatesForDates(List<string> dates);

  Task<Result<decimal>> GetRate(string fromCurrency, string toCurrency, string date);

  Task<Result<ExchangeRates>> GetExchangeRatesByDate(string date);
}
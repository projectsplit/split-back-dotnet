using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExchangeRateRepository;

public interface IExchangeRateRepository
{
  Task<Result> GetExchangeRatesFromExternalProvider(string baseCurrency, string date);

  Task<List<ExchangeRates>> GetAllRatesForDates(List<string> dates);

  Task<Result<ExchangeRates>> GetExchangeRatesByDate(string date);
}
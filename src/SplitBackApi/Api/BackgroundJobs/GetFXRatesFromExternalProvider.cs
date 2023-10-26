using Quartz;
using SplitBackApi.Data.Repositories.ExchangeRateRepository;
namespace SplitBackApi.Api.BackgroundJobs;

public class GetFXRatesFromExternalProvider : IJob
{
  private readonly IExchangeRateRepository _exchangeRateRepository;

  public GetFXRatesFromExternalProvider(IExchangeRateRepository exchangeRateRepository)
  {
    _exchangeRateRepository = exchangeRateRepository;
  }

  public async Task Execute(IJobExecutionContext context)
  {
    var currentDate = DateTime.UtcNow;
    await _exchangeRateRepository.GetExchangeRatesFromExternalProvider("USD", currentDate.AddDays(-1).ToString("yyyy-MM-dd"));

  }
}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SplitBackApi.Api.Endpoints.Authentication;
using SplitBackApi.Api.Endpoints.Comments;
using SplitBackApi.Api.Endpoints.Expenses;
using SplitBackApi.Api.Endpoints.Groups;
using SplitBackApi.Api.Endpoints.Invitations;
using SplitBackApi.Api.Endpoints.Permissions;
using SplitBackApi.Api.Endpoints.Transactions;
using SplitBackApi.Api.Endpoints.Transfers;
using SplitBackApi.Api.Endpoints.OpenAI;
using SplitBackApi.Api.Endpoints.Budgets;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Middlewares;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.CommentRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.InvitationRepository;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Services;
using SplitBackApi.Domain.Validators;
using SplitBackApi.Data.Repositories.GoogleUserRepository;
using SplitBackApi.Api.Services.GoogleAuthService;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Api.Services.HttpClients;

namespace SplitBackApi;

public class Program
{

  public static void Main(string[] args)
  {

    var builder = WebApplication.CreateBuilder(args);

    // Settings
    var configSection = builder.Configuration.GetSection(AppSettings.SectionName);
    builder.Services.Configure<AppSettings>(configSection);

    // Cors
    builder.Services.AddCors(options =>
    {
      options.AddDefaultPolicy(builder =>
      {
        builder.WithOrigins("http://localhost:3000")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
      });
    });

    builder.Services.AddHttpClient("openexchangerates",client =>
    {
      client.DefaultRequestHeaders.Add("accept", "application/json");
      client.BaseAddress = new Uri("https://openexchangerates.org/api/");
    });

    builder.Services.AddScoped<ExchangeRateClient>();

    // Repositories
    builder.Services.AddScoped<IUserRepository, UserMongoDbRepository>();
    builder.Services.AddScoped<IGoogleUserRepository, GoogleUserMongoDbRepository>();
    builder.Services.AddScoped<ISessionRepository, SessionMongoDbRepository>();
    builder.Services.AddScoped<IGroupRepository, GroupMongoDbRepository>();
    builder.Services.AddScoped<IExpenseRepository, ExpenseMongoDbRepository>();
    builder.Services.AddScoped<ITransferRepository, TransferMongoDbRepository>();
    builder.Services.AddScoped<ICommentRepository, CommentMongoDbRepository>();
    builder.Services.AddScoped<IInvitationRepository, InvitationMongoDbRepository>();
    builder.Services.AddScoped<IBudgetRepository, BudgetMongoDbRepository>();


    // Services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<GoogleAuthService>();
    builder.Services.AddScoped<TransactionService>();
    builder.Services.AddScoped<OpenAIService>();
    builder.Services.AddScoped<BudgetService>();

    builder.Services.AddHttpClient();

    // Validators
    builder.Services.AddScoped<SignInValidator>();
    builder.Services.AddScoped<SignUpValidator>();
    builder.Services.AddScoped<GroupValidator>();
    builder.Services.AddScoped<ExpenseValidator>();
    builder.Services.AddScoped<TransferValidator>();
    builder.Services.AddScoped<CommentValidator>();
    builder.Services.AddScoped<GuestMemberValidator>();
    builder.Services.AddScoped<UserMemberValidator>();
    builder.Services.AddScoped<EmailInitiateValidator>();
    builder.Services.AddScoped<BudgetValidator>();

    // Middlewares
    builder.Services.AddScoped<ExceptionHandlerMiddleware>();

    // Auth
    builder.Services.AddJwtBearerAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddAuthorization(options =>
    {
      options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
    });

    // https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSwaggerWithAutorization();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
      app.MapGet("/app-settings", (IOptions<AppSettings> appSettings) =>
      {
        return appSettings.Value;
      }).AllowAnonymous();
    }
    app.UseCors();
    app.UseHttpsRedirection();
    app.MapAuthenticationEndpoints();
    app.MapBudgetEndpoints();
    app.MapExpenseEndpoints();
    app.MapCommentEndpoints();
    app.MapTransferEndpoints();
    app.MapInvitationEndpoints();
    app.MapGroupEndpoints();
    app.MapGuestEndpoints();
    app.MapTransactionEndpoints();
    app.MapPermissionEndpoints();
    app.MapOpenAIEndpoints();
    app.UseMiddleware<ExceptionHandlerMiddleware>();
    app.UseAuthorization();

    app.Run();
  }
}
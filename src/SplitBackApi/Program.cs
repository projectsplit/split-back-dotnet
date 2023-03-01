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

namespace SplitBackApi;

public class Program {

  public static void Main(string[] args) {

    var builder = WebApplication.CreateBuilder(args);

    // Settings
    var configSection = builder.Configuration.GetSection(AppSettings.SectionName);
    builder.Services.Configure<AppSettings>(configSection);

    // Repositories
    builder.Services.AddScoped<IUserRepository, UserMongoDbRepository>();
    builder.Services.AddScoped<ISessionRepository, SessionMongoDbRepository>();
    builder.Services.AddScoped<IGroupRepository, GroupMongoDbRepository>();
    builder.Services.AddScoped<IExpenseRepository, ExpenseMongoDbRepository>();
    builder.Services.AddScoped<ITransferRepository, TransferMongoDbRepository>();
    builder.Services.AddScoped<ICommentRepository, CommentMongoDbRepository>();
    builder.Services.AddScoped<IInvitationRepository, InvitationMongoDbRepository>();
    
    // Services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<TransactionService>();
    
    // Validators
    builder.Services.AddScoped<GroupValidator>();
    builder.Services.AddScoped<ExpenseValidator>();
    builder.Services.AddScoped<TransferValidator>();
    
    // Middlewares
    builder.Services.AddScoped<ExceptionHandlerMiddleware>();

    // Auth
    builder.Services.AddJwtBearerAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddAuthorization(options => {
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

    if(app.Environment.IsDevelopment()) {
      app.UseSwagger();
      app.UseSwaggerUI();
      app.MapGet("/appsettings", (IOptions<AppSettings> appSettings) => {
        return appSettings.Value;
      });
    }

    app.UseHttpsRedirection();
    app.MapAuthenticationEndpoints();
    app.MapExpenseEndpoints();
    app.MapCommentEndpoints();
    app.MapTransferEndpoints();
    app.MapInvitationEndpoints();
    app.MapGroupEndpoints();
    app.MapTransactionEndpoints();
    app.MapPermissionEndpoints();
    app.UseMiddleware<ExceptionHandlerMiddleware>();
    app.UseAuthorization();

    app.Run();
  }
}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Endpoints;
using SplitBackApi.Extensions;

namespace SplitBackApi;

public class Program {

  public static void Main(string[] args) {

    var builder = WebApplication.CreateBuilder(args);

    var configSection = builder.Configuration.GetSection(AppSettings.SectionName);
    builder.Services.Configure<AppSettings>(configSection);

    builder.Services.AddScoped<IGroupRepository, GroupMongoDbRepository>();
    builder.Services.AddScoped<IExpenseRepository, ExpenseMongoDbRepository>();
    builder.Services.AddScoped<ITransferRepository, TransferMongoDbRepository>();
    builder.Services.AddScoped<ICommentRepository, CommentMongoDbRepository>();
    builder.Services.AddScoped<IInvitationRepository, InvitationMongoDbRepository>();
    builder.Services.AddScoped<IUserRepository, UserMongoDbRepository>();
    builder.Services.AddScoped<ISessionRepository, SessionMongoDbRepository>();
    
    
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<TransactionService>();
    
    builder.Services.AddScoped<GroupValidator>();
    builder.Services.AddScoped<ExpenseValidator>();
    builder.Services.AddScoped<TransferValidator>();
    
    builder.Services.AddScoped<ExceptionHandlerMiddleware>();

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
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
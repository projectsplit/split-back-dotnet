using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Endpoints;
using SplitBackApi.Extensions;
using MongoDB.Bson;
using SplitBackApi.Endpoints.Requests;

namespace SplitBackApi;

public class Program {

  public static void Main(string[] args) {

    var builder = WebApplication.CreateBuilder(args);

    var configSection = builder.Configuration.GetSection(AppSettings.SectionName);
    builder.Services.Configure<AppSettings>(configSection);

    builder.Services.AddScoped<IRepository, MongoDbRepository>();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<PermissionCheckMiddleware>();


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
    builder.Services.AddScoped<RoleService>();

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
    app.MapTransferEndpoints();
    app.MapInvitationEndpoints();
    app.MapRolesEndpoints();
    app.MapGroupEndpoints();
    app.UseAuthorization();
    app.UsePermissions();

    app.Run();
  }

}

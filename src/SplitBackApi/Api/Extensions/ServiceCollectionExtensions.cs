using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SplitBackApi.Configuration;
using Quartz;
using SplitBackApi.Api.BackgroundJobs;

namespace SplitBackApi.Api.Extensions;

public static class ServiceCollectionExtensions
{

  public static void AddJwtBearerAuthentication(this IServiceCollection services)
  {

    var jwtSettings = services.BuildServiceProvider()
      .GetRequiredService<IOptions<AppSettings>>().Value.Jwt;

    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidIssuer = jwtSettings.Issuer,
      ValidateIssuer = true,
      ValidAudience = jwtSettings.Audience,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings.Key)),
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero
    };

    services.AddAuthentication(options =>
    {
      // options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      // options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
      options.TokenValidationParameters = tokenValidationParameters;
    });
  }


  public static void AddSwaggerWithAutorization(this IServiceCollection services)
  {

    var securityScheme = new OpenApiSecurityScheme()
    {
      Name = "Authorization",
      Type = SecuritySchemeType.ApiKey,
      Scheme = "Bearer",
      BearerFormat = "JWT",
      In = ParameterLocation.Header,
      Description = "JSON Web Token based security",
    };

    var securityReq = new OpenApiSecurityRequirement() {
      {
        new OpenApiSecurityScheme {
          Reference = new OpenApiReference {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          }
        },
        Array.Empty<string>()
      }
    };

    var contactInfo = new OpenApiContact()
    {
      Name = "thanoskat",
      Email = "thanoskat@email.com",
      Url = new Uri("http://github.com/thanoskat")
    };

    var license = new OpenApiLicense()
    {
      Name = "Free License",
    };

    var info = new OpenApiInfo()
    {
      Version = "V1",
      Title = "Split Back MinimalApi",
      Description = "Api for split back",
      Contact = contactInfo,
      License = license
    };

    services.AddSwaggerGen(options =>
    {
      options.SwaggerDoc("v1", info);
      options.AddSecurityDefinition("Bearer", securityScheme);
      options.AddSecurityRequirement(securityReq);
    });
  }

  public static void AddQuartz(this IServiceCollection services)
  {
    services.AddQuartz(options =>
    {
      options.UseMicrosoftDependencyInjectionJobFactory();

      var jobKey = new JobKey("getFXrates");
      options.AddJob<GetFXRatesFromExternalProvider>(opts => opts.WithIdentity(jobKey));

      options.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("getFXrates-trigger")
        .WithCronSchedule("0 0 0 ? * TUE-SAT")
    );
    });
    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
  }
}
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Voltix.Application.IServices;
using Voltix.Application.Services;
using Voltix.Infrastructure.SignlR;
using System.Security.Claims;
using System.Text;

namespace Voltix.API.Extentions;
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAuthenticationService(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddCookie("External", options =>
            {
                options.Cookie.Name = "ExternalLogin";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.Cookie.SameSite = SameSiteMode.None;
                options.SlidingExpiration = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("Canot find JWT secret"))),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/api/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = builder.Configuration["Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId is not configured.");
                options.ClientSecret = builder.Configuration["Google:ClientSecret"]
                    ?? throw new InvalidOperationException("Google ClientSecret is not configured.");

                options.CallbackPath = "/signin-google";

                options.SignInScheme = "External";

                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SaveTokens = true;
            });

        return builder;
    }

    public static WebApplicationBuilder AddSwaggerGen(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "Please enter your token with format: \"Bearer YOUR_TOKEN\"",
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference()
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }

                        },
                        new List<string>()
                    }
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddHttpVNPT(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IEContractService, EContractService>(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["EContractClient:BaseUrl"]
                ?? throw new Exception("Cannot find base url in EContractClient"));

            client.Timeout = TimeSpan.FromSeconds(300);

            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 2,
        AllowAutoRedirect = true,
        AutomaticDecompression = System.Net.DecompressionMethods.All,
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
    });

        return builder;
    }

    public static WebApplicationBuilder AddSignalR(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

        return builder;
    }
}

using Amazon.Extensions.NETCore.Setup;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Voltix.API.Extentions;
using Voltix.Application.IService;
using Voltix.Application.Service;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.Seeders;
using Voltix.Infrastructure.SignlR;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5000);
//    options.ListenAnyIP(5000);
//});

//builder.Configuration.AddSystemsManager("/swp391/prod/", reloadAfter: TimeSpan.FromMinutes(5));
//builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
//builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));

QuestPDF.Settings.License = LicenseType.Community;


// Add services to the container.

builder.Services.AddControllers();

//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();

// Register services
// Base on Extensions.ServiceCollectionExtensions
builder.Services.RegisterService();

// Register Google Authentication
// Base on Extensions.WebApplicationBuilderExtensions
//builder.AddGoogleAuthentication();

// Register Authentication service
// Base on Extensions.WebApplicationBuilderExtensions
builder.AddAuthenticationService();

// Register SignalR service
// Base on Extensions.ServiceCollectionExtensions
builder.AddSignalR();

// Register Swagger that has bearer services
// Base on Extensions.ServiceCollectionExtensions
builder.AddSwaggerGen();

builder.AddRedisCacheService();

builder.Services.AddMemoryCache();

builder.AddHttpVNPT();

var allowedOrigins = new[] {
    "http://localhost:5173",
    "http://localhost:3000"
};
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("FrontEnd", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
    );
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

var forwardOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = null
};
forwardOptions.KnownNetworks.Clear();
forwardOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardOptions);

app.UseHttpsRedirection();

if (app.Configuration.GetValue<bool>("Swagger:Enabled") || app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Method == HttpMethods.Head &&
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Request.Method = HttpMethods.Get;
        }
        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseRouting();

app.UseCors("FrontEnd");

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/api/notificationHub")
   .RequireCors("FrontEnd");

app.MapGet("/api/me", (HttpContext ctx) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();
    var name = ctx.User.Identity!.Name ?? "";
    var email = ctx.User.Claims.FirstOrDefault(c => c.Type.Contains("email", StringComparison.OrdinalIgnoreCase))?.Value ?? "";
    var role = ctx.User.Claims.FirstOrDefault(c =>
        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ?? "";
    return Results.Ok(new { name, email, role });
}).RequireAuthorization();

app.MapGet("/api/auth/google", (HttpContext ctx) =>
{
    var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = "/login-success"
    };
    return Results.Challenge(props, new[] { GoogleDefaults.AuthenticationScheme });
});

app.MapControllers();

// Seed Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRolesAsync(roleManager);
}

app.Run();

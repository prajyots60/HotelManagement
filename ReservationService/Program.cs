using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using FluentValidation;
using ReservationService.Authorization;
using ReservationService.Configuration;
using ReservationService.Data;
using ReservationService.Middleware;
using ReservationService.Repositories.Implementations;
using ReservationService.Repositories.Interfaces;
using ReservationService.Services.Implementations;
using ReservationService.Services.Interfaces;
using ReservationService.Validators.Guests;
using Serilog;

// Prevent inotify limit exceptions on Linux environments
Environment.SetEnvironmentVariable(
    "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE", "false");

Environment.SetEnvironmentVariable(
    "DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGuestDtoValidator>();

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ReservationDb")));

// Enables access to current HttpContext for bearer token propagation across microservices
builder.Services.AddHttpContextAccessor();

// Register scoped repositories
builder.Services.AddScoped<
    IReservationRepository,
    ReservationRepository>();

builder.Services.AddScoped<
    IGuestRepository,
    GuestRepository>();

// Register scoped GuestService
builder.Services.AddScoped<
    IGuestService,
    GuestService>();

// Register typed HttpClient for IReservationService pointing to HotelManagementService
var hotelServiceBaseUrl = builder.Configuration["DownstreamServices:HotelManagementService:BaseUrl"]
    ?? "http://localhost:7002/";

builder.Services.AddHttpClient<IReservationService, ReservationService.Services.Implementations.ReservationService>(
    client =>
    {
        client.BaseAddress = new Uri(hotelServiceBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddStandardResilienceHandler(options =>
    {
        // Total request timeout
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

        // Attempt timeout
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);

        // Configure retry policy for transient HTTP failures (safe idempotent calls)
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;

        // Circuit breaker
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    });

// Swagger UI with Bearer Authentication support (OpenAPI 3.x / Swashbuckle 11)
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Hotel Management - Reservation Service",
        Version = "v1",
        Description = "Hotel guests, reservations, and room availability APIs"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Do not add the word Bearer."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

// JWT Options & validation binding
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 bytes.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ReservationPolicies.GuestsRead, policy =>
        policy.RequireRole("Owner", "Manager", "Receptionist"));

    options.AddPolicy(ReservationPolicies.GuestsManage, policy =>
        policy.RequireRole("Owner", "Manager", "Receptionist"));

    options.AddPolicy(ReservationPolicies.GuestsDelete, policy =>
        policy.RequireRole("Owner", "Manager"));

    options.AddPolicy(ReservationPolicies.ReservationsRead, policy =>
        policy.RequireRole("Owner", "Manager", "Receptionist"));

    options.AddPolicy(ReservationPolicies.ReservationsManage, policy =>
        policy.RequireRole("Owner", "Manager", "Receptionist"));

    options.AddPolicy(ReservationPolicies.RoomAvailabilityRead, policy =>
        policy.RequireRole("Owner", "Manager", "Receptionist"));
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication MUST precede Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

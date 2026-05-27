using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Data;
using Api.Services;
using Api.Routes;
using Api.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────────
// Restricts which frontend origins can call the API.
// In production, AllowedOrigins should be set to the deployed Vercel URL via environment variable.
var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(","))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── DATABASE ──────────────────────────────────────────────────────────────────
// Uses SQLite for MVP simplicity. Swapping to PostgreSQL for production would
// require only changing this connection string and the EF Core provider package.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=mytodo.db"));

// ── DEPENDENCY INJECTION ──────────────────────────────────────────────────────
// Registers all repositories and services with scoped lifetime —
// a new instance is created per HTTP request.
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<ProjectService>();

builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<TaskService>();

builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<CommentService>();

// ── AUTHENTICATION ────────────────────────────────────────────────────────────
// Configures JWT bearer token authentication. All protected routes require a valid
// token signed with the configured secret key.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// ── AUTHORIZATION & RATE LIMITING ─────────────────────────────────────────────
// Rate limiting is applied only to auth endpoints to prevent brute force attacks.
// Allows 10 requests per minute per client — generous for real users, blocking for scripts.
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
// Order matters here — error handling wraps everything, CORS must come before
// auth, and authentication must come before authorization.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ── ROUTES ────────────────────────────────────────────────────────────────────
app.MapAuthRoutes();
app.MapProjectRoutes();
app.MapTaskRoutes();
app.MapCommentRoutes();
app.MapUserRoutes();

app.Run();

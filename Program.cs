using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Proyecto_GIPE.Models.Entities;
using Proyecto_GIPE.Models.IRepositorios;
using Proyecto_GIPE.Models.Repositorios;
using Proyecto_GIPE.Models.Servicios;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = new MySqlConnectionStringBuilder
{
    Server = GetEnv("DB_HOST", "localhost"),
    Port = uint.Parse(GetEnv("DB_PORT", "3306")),
    Database = GetEnv("DB_NAME", "bdgipe"),
    UserID = GetEnv("DB_USER", "root"),
    Password = GetEnv("DB_PASSWORD", ""),
    CharacterSet = "utf8mb4"
}.ConnectionString;

builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

var jwtSecret = GetEnv("JWT_SECRET");
var jwtIssuer = GetEnv("JWT_ISSUER", "GIPE");
var jwtAudience = GetEnv("JWT_AUDIENCE", "GIPE");

builder.Configuration["TokenAuthentication:SecretKey"] = jwtSecret;
builder.Configuration["TokenAuthentication:Issuer"] = jwtIssuer;
builder.Configuration["TokenAuthentication:Audience"] = jwtAudience;

builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<ServicioSeed>();
builder.Services.AddScoped<ServicioJwt>();

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "GIPE.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ReturnUrlParameter = "returnUrl";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Dueno", policy => policy.RequireRole("Dueno"));
    options.AddPolicy("Empleado", policy => policy.RequireRole("Dueno", "Empleado"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ServicioSeed>().Ejecutar();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("ApiCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static string GetEnv(string name, string? fallback = null)
{
    var value = Environment.GetEnvironmentVariable(name);

    if (!string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    if (fallback is not null)
    {
        return fallback;
    }

    throw new InvalidOperationException(
        $"Falta la variable de entorno {name}. Revisá el archivo .env.");
}

using System.Text;
using System.Text.Json.Serialization;
using FondoInversion.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


// Agregar DbContext con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Registro de repositorios 
builder.Services.AddTransient<IEncryptionService, AesEncryptionService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IPrecioRepository, PrecioRepository>();
builder.Services.AddTransient<IFlujoRepository, FlujoRepository>();
builder.Services.AddTransient<ITokenRepository, TokenRepository>();
builder.Services.AddTransient<IEventLogRepository, EventLogRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<ICSVService, CSVService>();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();

// Registro de servicios
builder.Services.AddTransient<AesEncryptionService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<PrecioService>();
builder.Services.AddTransient<FlujoService>();
builder.Services.AddTransient<TokenService>();
builder.Services.AddTransient<EventLogService>();
builder.Services.AddTransient<CurrentUserService>();

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<JwtAuthorizeAttribute>();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

// Cors
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(["http://localhost:4200", "http://localhost:5206", "https://fondo-inversion-front-991595227055.northamerica-south1.run.app"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });

    
});

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions( x=> x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles) ;
builder.Services.AddOpenApi();


var app = builder.Build();

//Habilirr swagger solo informativo para desarrollo ???
// if (app.Environment.IsDevelopment())
// {
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Mi API v1");
        options.RoutePrefix = "swagger"; 
        options.DocumentTitle = "Mi API Documentation";
        options.EnablePersistAuthorization();
    });
// }
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseCors(MyAllowSpecificOrigins);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Evento para la salida 

app.Run();


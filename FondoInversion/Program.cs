using System.Text;
using System.Text.Json.Serialization;
using FondoInversion.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Registro de repositorios 
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IPrecioRepository, PrecioRepository>();
builder.Services.AddTransient<IFlujoRepository, FlujoRepository>();
builder.Services.AddTransient<ITokenRepository, TokenRepository>();
builder.Services.AddTransient<IEventLogRepository, EventLogRepository>();

// Registro de servicios
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<PrecioService>();
builder.Services.AddTransient<FlujoService>();
builder.Services.AddTransient<TokenService>();
builder.Services.AddTransient<EventLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<ICSVService, CSVService>();


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
            policy.WithOrigins(["http://localhost:4200", "https://fondo-inversion-front-991595227055.northamerica-south1.run.app"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions( x=> x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles) ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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


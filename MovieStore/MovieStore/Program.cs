using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT için
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens; // TokenValidationParameters için
using Microsoft.OpenApi.Models;      // OpenApiInfo ve SecurityScheme için
// Örnek bir handler/command tipi (projenizdeki gerçek bir tiple deðiþtirin veya Assembly.GetExecutingAssembly() kullanýn)
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies;
using MovieStore.MovieStore.API.Cqrs.Validations; // ActorValidator veya projenizdeki herhangi bir validator
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Mappings;
using System.Reflection;
using System.Text; // Encoding.UTF8 için

var builder = WebApplication.CreateBuilder(args);

// 1. Servislerin Konfigürasyonu (Dependency Injection)

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MapperConfig));

// MediatR'ý ekleyin
// Tüm handler'larýnýzýn ve sorgularýnýzýn/komutlarýnýzýn
// Program.cs'in bulunduðu ana projenizin assembly'sinde olduðunu varsayarsak:
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
// Veya belirli bir tipin bulunduðu assembly'yi taramak için (örneðin, handler'larýnýz farklý bir projede ise):
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateMovieCommandHandler).Assembly));


// FluentValidation'ý ekleyin
builder.Services.AddFluentValidationAutoValidation();
// Tüm validator'larýnýzýn ayný assembly'de olduðunu varsayarsak (örneðin ActorValidator'ýn bulunduðu assembly):
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
// Veya tüm validator'larý içeren ana projenizin assembly'sini belirtmek için:
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI için servisleri yapýlandýrýn
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Maðazasý API'si için ASP.NET Core Web API (.NET 8)"
        // Ýsteðe baðlý olarak iletiþim ve lisans bilgileri eklenebilir
        // Contact = new OpenApiContact { Name = "Geliþtirici Adý", Email = "email@example.com" },
        // License = new OpenApiLicense { Name = "Lisans Türü", Url = new Uri("https://example.com/license") }
    });

    // Swagger UI'a JWT Authentication desteði eklemek için Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Authorization: Bearer 12345abcdef\"",
        Name = "Authorization", // Header adý
        In = ParameterLocation.Header, // Token'ýn nerede gönderileceði (Header)
        Type = SecuritySchemeType.ApiKey, // Güvenlik þemasý tipi
        Scheme = "Bearer" // Þema adý
    });

    // Swagger UI'a JWT Authentication desteði eklemek için Security Requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, // Referans tipi
                    Id = "Bearer" // Yukarýda AddSecurityDefinition'da tanýmlanan ID
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>() // Gerekli scope'lar (bu örnekte boþ)
        }
    });
});


// JWT Authentication Servislerini Ekleme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true; // Token'ý HttpContext.User.Identity altýnda saklamak için (isteðe baðlý)
    options.RequireHttpsMetadata = false; // Geliþtirme ortamýnda false olabilir, üretimde true olmalý
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Token'ý yayýnlayaný doðrula
        ValidateAudience = true, // Token'ýn hedef kitlesini doðrula
        ValidateLifetime = true, // Token'ýn süresinin dolup dolmadýðýný doðrula
        ValidateIssuerSigningKey = true, // Token'ý imzalayan anahtarý doðrula

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // appsettings.json'dan alýnacak
        ValidAudience = builder.Configuration["JwtSettings:Audience"], // appsettings.json'dan alýnacak
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json."))),

        ClockSkew = TimeSpan.Zero // Token'ýn süresi dolduktan sonra ek tolerans süresi (varsayýlan 5dk, 0 anýnda geçersiz kýlar)
    };
});

// Authorization servislerini ekleme (Rol bazlý vs. için temel)
builder.Services.AddAuthorization();


var app = builder.Build();

// 2. HTTP Ýstek Pipeline'ýnýn Konfigürasyonu

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geliþtirme ortamýnda detaylý hata sayfalarý
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Swagger JSON endpoint'inin doðru olduðundan emin olun
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API V1");
        // RoutePrefix'i boþ string yapmak, Swagger UI'ý kök dizinde (örn: https://localhost:PORT/) sunar
        options.RoutePrefix = string.Empty;
    });
}
else
{
    // Üretim ortamý için genel hata yönetimi middleware'i eklenebilir
    // app.UseExceptionHandler("/Error"); 
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'e yönlendir

// ÖNEMLÝ: Authentication middleware'i, Authorization middleware'inden ÖNCE gelmelidir.
app.UseAuthentication(); // Kimlik doðrulama middleware'ini etkinleþtir
app.UseAuthorization();  // Yetkilendirme middleware'ini etkinleþtir

app.MapControllers(); // Controller endpoint'lerini eþle

app.Run();

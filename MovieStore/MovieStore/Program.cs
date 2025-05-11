using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.MoviesActors;
using MovieStore.MovieStore.API.Cqrs.Validations; // Birden fazla validator için tek bir using yeterli olabilir
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Mappings;
using System.Reflection;
using System.Text;

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
// Eðer GetActiveActorsForMovieQuery farklý bir assembly'de ise, onun assembly'sini belirtin:
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetActiveActorsForMovieQuery).Assembly));


// FluentValidation'ý ekleyin
builder.Services.AddFluentValidationAutoValidation();
// Tüm validator'larýnýzýn ActorValidator ile ayný assembly'de olduðunu varsayarsak:
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
// Veya tüm validator'larý içeren assembly'yi belirtmek için:
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Maðazasý API'si için ASP.NET Core Web API (.NET 8)"
    });

    // Swagger UI'a JWT Authentication desteði eklemek için
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


// JWT Authentication Servislerini Ekleme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
    };
});

// Authorization servislerini ekleme (Rol bazlý vs. için temel)
builder.Services.AddAuthorization();



builder.Services.AddEndpointsApiExplorer();
// SwaggerGen için temel ayarlar. Gerekirse daha fazla konfigürasyon eklenebilir.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Maðazasý API'si için ASP.NET Core Web API (.NET 8)"
    });
});

var app = builder.Build();

// 2. HTTP Ýstek Pipeline'ýnýn Konfigürasyonu

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    // Swagger UI'ý kök dizinde sunmak için ayar:
    app.UseSwaggerUI(options =>
    {
        // Swagger JSON endpoint'inin doðru olduðundan emin olun
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API V1");
        // RoutePrefix'i boþ string yapmak, Swagger UI'ý kök dizinde (örn: https://localhost:7082/) sunar
        options.RoutePrefix = string.Empty;
    });
}
else
{
    // Üretim ortamý için hata yönetimi (isteðe baðlý, daha genel bir hata sayfasý olabilir)
    // app.UseExceptionHandler("/Error"); // Özel bir hata sayfasýna yönlendirme
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();
app.UseAuthorization(); // Eðer Authentication kullanýyorsanýz, app.UseAuthentication()'dan sonra gelmeli
app.MapControllers();
app.Run();

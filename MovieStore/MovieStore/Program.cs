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
using MovieStore.MovieStore.API.Cqrs.Validations; // Birden fazla validator i�in tek bir using yeterli olabilir
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Mappings;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Servislerin Konfig�rasyonu (Dependency Injection)

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MapperConfig));

// MediatR'� ekleyin
// T�m handler'lar�n�z�n ve sorgular�n�z�n/komutlar�n�z�n
// Program.cs'in bulundu�u ana projenizin assembly'sinde oldu�unu varsayarsak:
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
// E�er GetActiveActorsForMovieQuery farkl� bir assembly'de ise, onun assembly'sini belirtin:
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetActiveActorsForMovieQuery).Assembly));


// FluentValidation'� ekleyin
builder.Services.AddFluentValidationAutoValidation();
// T�m validator'lar�n�z�n ActorValidator ile ayn� assembly'de oldu�unu varsayarsak:
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
// Veya t�m validator'lar� i�eren assembly'yi belirtmek i�in:
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Ma�azas� API'si i�in ASP.NET Core Web API (.NET 8)"
    });

    // Swagger UI'a JWT Authentication deste�i eklemek i�in
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

// Authorization servislerini ekleme (Rol bazl� vs. i�in temel)
builder.Services.AddAuthorization();



builder.Services.AddEndpointsApiExplorer();
// SwaggerGen i�in temel ayarlar. Gerekirse daha fazla konfig�rasyon eklenebilir.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Ma�azas� API'si i�in ASP.NET Core Web API (.NET 8)"
    });
});

var app = builder.Build();

// 2. HTTP �stek Pipeline'�n�n Konfig�rasyonu

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    // Swagger UI'� k�k dizinde sunmak i�in ayar:
    app.UseSwaggerUI(options =>
    {
        // Swagger JSON endpoint'inin do�ru oldu�undan emin olun
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API V1");
        // RoutePrefix'i bo� string yapmak, Swagger UI'� k�k dizinde (�rn: https://localhost:7082/) sunar
        options.RoutePrefix = string.Empty;
    });
}
else
{
    // �retim ortam� i�in hata y�netimi (iste�e ba�l�, daha genel bir hata sayfas� olabilir)
    // app.UseExceptionHandler("/Error"); // �zel bir hata sayfas�na y�nlendirme
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();
app.UseAuthorization(); // E�er Authentication kullan�yorsan�z, app.UseAuthentication()'dan sonra gelmeli
app.MapControllers();
app.Run();

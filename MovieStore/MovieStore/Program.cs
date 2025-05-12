using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT i�in
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens; // TokenValidationParameters i�in
using Microsoft.OpenApi.Models;      // OpenApiInfo ve SecurityScheme i�in
// �rnek bir handler/command tipi (projenizdeki ger�ek bir tiple de�i�tirin veya Assembly.GetExecutingAssembly() kullan�n)
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies;
using MovieStore.MovieStore.API.Cqrs.Validations; // ActorValidator veya projenizdeki herhangi bir validator
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Mappings;
using System.Reflection;
using System.Text; // Encoding.UTF8 i�in

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
// Veya belirli bir tipin bulundu�u assembly'yi taramak i�in (�rne�in, handler'lar�n�z farkl� bir projede ise):
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateMovieCommandHandler).Assembly));


// FluentValidation'� ekleyin
builder.Services.AddFluentValidationAutoValidation();
// T�m validator'lar�n�z�n ayn� assembly'de oldu�unu varsayarsak (�rne�in ActorValidator'�n bulundu�u assembly):
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
// Veya t�m validator'lar� i�eren ana projenizin assembly'sini belirtmek i�in:
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI i�in servisleri yap�land�r�n
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Ma�azas� API'si i�in ASP.NET Core Web API (.NET 8)"
        // �ste�e ba�l� olarak ileti�im ve lisans bilgileri eklenebilir
        // Contact = new OpenApiContact { Name = "Geli�tirici Ad�", Email = "email@example.com" },
        // License = new OpenApiLicense { Name = "Lisans T�r�", Url = new Uri("https://example.com/license") }
    });

    // Swagger UI'a JWT Authentication deste�i eklemek i�in Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Authorization: Bearer 12345abcdef\"",
        Name = "Authorization", // Header ad�
        In = ParameterLocation.Header, // Token'�n nerede g�nderilece�i (Header)
        Type = SecuritySchemeType.ApiKey, // G�venlik �emas� tipi
        Scheme = "Bearer" // �ema ad�
    });

    // Swagger UI'a JWT Authentication deste�i eklemek i�in Security Requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, // Referans tipi
                    Id = "Bearer" // Yukar�da AddSecurityDefinition'da tan�mlanan ID
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>() // Gerekli scope'lar (bu �rnekte bo�)
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
    options.SaveToken = true; // Token'� HttpContext.User.Identity alt�nda saklamak i�in (iste�e ba�l�)
    options.RequireHttpsMetadata = false; // Geli�tirme ortam�nda false olabilir, �retimde true olmal�
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Token'� yay�nlayan� do�rula
        ValidateAudience = true, // Token'�n hedef kitlesini do�rula
        ValidateLifetime = true, // Token'�n s�resinin dolup dolmad���n� do�rula
        ValidateIssuerSigningKey = true, // Token'� imzalayan anahtar� do�rula

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // appsettings.json'dan al�nacak
        ValidAudience = builder.Configuration["JwtSettings:Audience"], // appsettings.json'dan al�nacak
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json."))),

        ClockSkew = TimeSpan.Zero // Token'�n s�resi dolduktan sonra ek tolerans s�resi (varsay�lan 5dk, 0 an�nda ge�ersiz k�lar)
    };
});

// Authorization servislerini ekleme (Rol bazl� vs. i�in temel)
builder.Services.AddAuthorization();


var app = builder.Build();

// 2. HTTP �stek Pipeline'�n�n Konfig�rasyonu

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geli�tirme ortam�nda detayl� hata sayfalar�
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Swagger JSON endpoint'inin do�ru oldu�undan emin olun
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API V1");
        // RoutePrefix'i bo� string yapmak, Swagger UI'� k�k dizinde (�rn: https://localhost:PORT/) sunar
        options.RoutePrefix = string.Empty;
    });
}
else
{
    // �retim ortam� i�in genel hata y�netimi middleware'i eklenebilir
    // app.UseExceptionHandler("/Error"); 
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'e y�nlendir

// �NEML�: Authentication middleware'i, Authorization middleware'inden �NCE gelmelidir.
app.UseAuthentication(); // Kimlik do�rulama middleware'ini etkinle�tir
app.UseAuthorization();  // Yetkilendirme middleware'ini etkinle�tir

app.MapControllers(); // Controller endpoint'lerini e�le

app.Run();

// Gerekli using ifadeleri
using FluentValidation; // FluentValidation için
using FluentValidation.AspNetCore; // FluentValidation ASP.NET Core entegrasyonu için
using MediatR; // MediatR için
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies; // MovieCommandHandler için
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies; // MovieQueryHandler için
using MovieStore.MovieStore.API.Cqrs.Validations; // ActorValidator, MovieValidator için
using MovieStore.MovieStore.API.DbContexts; // AppDbContext için
using MovieStore.MovieStore.API.Mappings; // MapperConfig için
using MovieStore.MovieStore.API.Cqrs.Validations;
using MovieStore.MovieStore.API.Cqrs.Validations;
using System.Reflection; // Assembly.GetExecutingAssembly() (kullanýlmýyorsa kaldýrýlabilir)
// using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT için gerekirse
// using Microsoft.IdentityModel.Tokens; // JWT için gerekirse
// using System.Text; // JWT için gerekirse

var builder = WebApplication.CreateBuilder(args);

// 1. Servislerin Konfigürasyonu (Dependency Injection)

// MVC Kontrolcüleri için servisleri ekleyin
builder.Services.AddControllers();

// Entity Framework Core DbContext'inizi ekleyin
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper'ý ekleyin (MapperConfig sýnýfýnýzýn bulunduðu assembly'yi kullanýr)
builder.Services.AddAutoMapper(typeof(MapperConfig));

// MediatR'ý ekleyin (Belirtilen handler sýnýflarýnýn bulunduðu assembly'leri tarar)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MovieCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(MovieQueryHandler).Assembly);
    // Eðer tüm handler'lar ayný assembly'de ise:
    // cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// FluentValidation'ý ekleyin
// ASP.NET Core entegrasyonunu (örn: otomatik model durumu doðrulamasý) saðlar.
builder.Services.AddFluentValidationAutoValidation();
// Belirli validator sýnýflarýný içeren assembly'lerden validator'larý kaydeder.
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MovieValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AssignActorToMovieRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DirectorValidator>();
// Alternatif olarak, eðer tüm validator'lar ayný assembly'de ise:
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


// Swagger/OpenAPI için servisleri ekleyin
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Maðazasý API'si için ASP.NET Core Web API (.NET 8)",
        // Aþaðýdaki kýsýmlarý kendi projenize göre doldurabilirsiniz
        // TermsOfService = new Uri("https://example.com/terms"),
        // Contact = new Microsoft.OpenApi.Models.OpenApiContact
        // {
        //     Name = "Geliþtirici Adý",
        //     Email = "iletisim@example.com",
        // },
        // License = new Microsoft.OpenApi.Models.OpenApiLicense
        // {
        //     Name = "MIT Lisansý",
        //     Url = new Uri("https://opensource.org/licenses/MIT"),
        // }
    });

    // Eðer XML yorumlarýný Swagger'a dahil etmek isterseniz:
    // 1. Projenizin .csproj dosyasýnda <GenerateDocumentationFile>true</GenerateDocumentationFile> ekleyin.
    // 2. Aþaðýdaki satýrlarýn yorumunu kaldýrýn:
    // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// JWT Kimlik Doðrulama (ihtiyaç duyulursa yorumu kaldýrýn ve yapýlandýrýn)
/*
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Token:Issuer"],
            ValidAudience = builder.Configuration["Token:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
            ClockSkew = TimeSpan.Zero
        };
    });
*/

var app = builder.Build();

// 2. HTTP Ýstek Pipeline'ýnýn Konfigürasyonu

// Geliþtirme ortamý için özel ayarlar
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detaylý hata sayfalarýný gösterir
    app.UseSwagger(); // Swagger middleware'ini etkinleþtirir
    app.UseSwaggerUI(options => // Swagger UI'ý yapýlandýrýr
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API V1");
        options.RoutePrefix = string.Empty; // Swagger UI'ý uygulamanýn kök dizininde sunar (örn: https://localhost:port/)
    });
}
else
{
    // Üretim ortamý için hata yönetimi (isteðe baðlý, daha genel bir hata sayfasý olabilir)
    // app.UseExceptionHandler("/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// HTTPS yönlendirmesini etkinleþtirin
app.UseHttpsRedirection();

// Kimlik doðrulama middleware'ini ekleyin (JWT veya diðerleri için)
// app.UseAuthentication(); // Bu satýr UseAuthorization'dan ÖNCE gelmelidir

// Yetkilendirme middleware'ini etkinleþtirin
app.UseAuthorization();

// Kontrolcü rotalarýný eþleyin
app.MapControllers();

// Örnek bir minimal API endpoint'i (isteðe baðlý)
app.MapGet("/health", () => Results.Ok(new { status = "API Çalýþýyor" }));

app.Run();

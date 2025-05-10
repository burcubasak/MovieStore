// Gerekli using ifadeleri
using FluentValidation; // FluentValidation i�in
using FluentValidation.AspNetCore; // FluentValidation ASP.NET Core entegrasyonu i�in
using MediatR; // MediatR i�in
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies; // MovieCommandHandler i�in
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies; // MovieQueryHandler i�in
using MovieStore.MovieStore.API.Cqrs.Validations; // ActorValidator, MovieValidator i�in
using MovieStore.MovieStore.API.DbContexts; // AppDbContext i�in
using MovieStore.MovieStore.API.Mappings; // MapperConfig i�in
using MovieStore.MovieStore.API.Cqrs.Validations;
using MovieStore.MovieStore.API.Cqrs.Validations;
using System.Reflection; // Assembly.GetExecutingAssembly() (kullan�lm�yorsa kald�r�labilir)
// using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT i�in gerekirse
// using Microsoft.IdentityModel.Tokens; // JWT i�in gerekirse
// using System.Text; // JWT i�in gerekirse

var builder = WebApplication.CreateBuilder(args);

// 1. Servislerin Konfig�rasyonu (Dependency Injection)

// MVC Kontrolc�leri i�in servisleri ekleyin
builder.Services.AddControllers();

// Entity Framework Core DbContext'inizi ekleyin
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper'� ekleyin (MapperConfig s�n�f�n�z�n bulundu�u assembly'yi kullan�r)
builder.Services.AddAutoMapper(typeof(MapperConfig));

// MediatR'� ekleyin (Belirtilen handler s�n�flar�n�n bulundu�u assembly'leri tarar)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MovieCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(MovieQueryHandler).Assembly);
    // E�er t�m handler'lar ayn� assembly'de ise:
    // cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// FluentValidation'� ekleyin
// ASP.NET Core entegrasyonunu (�rn: otomatik model durumu do�rulamas�) sa�lar.
builder.Services.AddFluentValidationAutoValidation();
// Belirli validator s�n�flar�n� i�eren assembly'lerden validator'lar� kaydeder.
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MovieValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AssignActorToMovieRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DirectorValidator>();
// Alternatif olarak, e�er t�m validator'lar ayn� assembly'de ise:
// builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


// Swagger/OpenAPI i�in servisleri ekleyin
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "MovieStore API",
        Description = "Film Ma�azas� API'si i�in ASP.NET Core Web API (.NET 8)",
        // A�a��daki k�s�mlar� kendi projenize g�re doldurabilirsiniz
        // TermsOfService = new Uri("https://example.com/terms"),
        // Contact = new Microsoft.OpenApi.Models.OpenApiContact
        // {
        //     Name = "Geli�tirici Ad�",
        //     Email = "iletisim@example.com",
        // },
        // License = new Microsoft.OpenApi.Models.OpenApiLicense
        // {
        //     Name = "MIT Lisans�",
        //     Url = new Uri("https://opensource.org/licenses/MIT"),
        // }
    });

    // E�er XML yorumlar�n� Swagger'a dahil etmek isterseniz:
    // 1. Projenizin .csproj dosyas�nda <GenerateDocumentationFile>true</GenerateDocumentationFile> ekleyin.
    // 2. A�a��daki sat�rlar�n yorumunu kald�r�n:
    // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// JWT Kimlik Do�rulama (ihtiya� duyulursa yorumu kald�r�n ve yap�land�r�n)
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

// 2. HTTP �stek Pipeline'�n�n Konfig�rasyonu

// Geli�tirme ortam� i�in �zel ayarlar
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detayl� hata sayfalar�n� g�sterir
    app.UseSwagger(); // Swagger middleware'ini etkinle�tirir
    app.UseSwaggerUI(options => // Swagger UI'� yap�land�r�r
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API V1");
        options.RoutePrefix = string.Empty; // Swagger UI'� uygulaman�n k�k dizininde sunar (�rn: https://localhost:port/)
    });
}
else
{
    // �retim ortam� i�in hata y�netimi (iste�e ba�l�, daha genel bir hata sayfas� olabilir)
    // app.UseExceptionHandler("/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// HTTPS y�nlendirmesini etkinle�tirin
app.UseHttpsRedirection();

// Kimlik do�rulama middleware'ini ekleyin (JWT veya di�erleri i�in)
// app.UseAuthentication(); // Bu sat�r UseAuthorization'dan �NCE gelmelidir

// Yetkilendirme middleware'ini etkinle�tirin
app.UseAuthorization();

// Kontrolc� rotalar�n� e�leyin
app.MapControllers();

// �rnek bir minimal API endpoint'i (iste�e ba�l�)
app.MapGet("/health", () => Results.Ok(new { status = "API �al���yor" }));

app.Run();

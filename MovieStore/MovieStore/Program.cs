using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Actors;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Actors;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies;
using MovieStore.MovieStore.API.Cqrs.Validations;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Mappings;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MovieStore API",
        Version = "v1"
    });
});
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddValidatorsFromAssemblyContaining<ActorValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MovieValidator>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR'ý servis konteynerine ekleyin
builder.Services.AddMediatR(cfg =>
{
    // Sadece gerekli assembly'leri tarayýn
    cfg.RegisterServicesFromAssembly(typeof(MovieCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(MovieQueryHandler).Assembly);
});

// Uncomment and configure JWT authentication if needed
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
// {
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Token:Issuer"],
//        ValidAudience = builder.Configuration["Token:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
//        ClockSkew = TimeSpan.Zero
//    };
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
// app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieStore API v1");
    c.RoutePrefix = string.Empty;
});

app.Run();

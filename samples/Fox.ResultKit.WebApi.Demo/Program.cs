//==================================================================================================
// Fox.ResultKit WebApi Demo application entry point.
// Demonstrates Classic Service Layer and CQRS with MediatR integration.
//==================================================================================================
using Fox.ResultKit.MediatR;
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using Fox.ResultKit.WebApi.Demo.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Repositories
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// Services (Classic approach)
builder.Services.AddScoped<UserService>();

// MediatR (CQRS approach)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
});

// ResultKit MediatR Pipeline Behavior
builder.Services.AddResultKitMediatR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "ResultKit WebApi Demo", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();


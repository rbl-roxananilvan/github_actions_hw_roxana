using Common;
using DataPersistance.Repositories;
using Interfaces;
using Roxana_tema1.Helpers;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IBookRepositoryAsync, BookRepositoryAsync>();
builder.Services.AddScoped<IBookServiceAsync, BookServiceAsync>();
builder.Services.ConfigureAppServices(builder.Configuration);
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.Configure<AuthorizationSettings>(
    builder.Configuration.GetSection("AuthorizationSettings"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Books");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Books V2");

        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

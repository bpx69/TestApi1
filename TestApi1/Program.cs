using Microsoft.EntityFrameworkCore;
using NLog.Web;
using TestApi1.Authentication;
using TestApi1.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c)=>c.OperationFilter<SwaggerHeaderFilter>());
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UserAPIDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiKeyVerifier>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

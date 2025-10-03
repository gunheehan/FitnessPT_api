using System.Text.Json.Serialization;
using FitnessPT_api.Data;
using FitnessPT_api.GoogleAuth;
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using FitnessPT_api.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MySQL 연결
var connectionString = builder.Configuration.GetConnectionString("FITNESSPT:ConnectionStrings:DatabaseConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Controllers 추가
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Google Auth 모듈 추가
builder.Services.AddGoogleAuthModule(builder.Configuration);
// IUserRepository 구현체 등록
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
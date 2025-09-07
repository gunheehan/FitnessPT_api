using System.Text.Json.Serialization;
using FitnessPT_api.Data;
using FitnessPT_api.GoogleAuth;
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL 연결 설정
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    }));

// Controllers 추가
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // JSON 직렬화 옵션 설정
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase 유지
        options.JsonSerializerOptions.WriteIndented = true; // 개발 시 가독성을 위해
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Enum을 문자열로 직렬화
    });

// Google Auth 모듈 추가
builder.Services.AddGoogleAuthModule(builder.Configuration);

// IUserRepository 구현체 등록
builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();

// Swagger 기본 설정만
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FitnessPT Google Auth API",
        Version = "v1"
    });
});

var app = builder.Build();

// 개발 환경에서만 Swagger 사용
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}
app.UseRouting();

// 정적 파일 서빙 추가 ⭐
//app.UseStaticFiles();

// 기본 API 정보 엔드포인트
app.MapGet("/", () => new
{
    name = "FitnessPT API",
    version = "1.0.0",
    description = "피트니스 PT 관리를 위한 REST API",
    timestamp = DateTime.UtcNow,
    endpoints = new
    {
        exercises = "/api/exercises - 운동 관리",
        categories = "/api/categories - 운동 카테고리 관리", 
        users = "/api/users - 사용자 관리",
        userProfiles = "/api/userprofiles - 사용자 프로필 관리",
        workoutRecords = "/api/workoutrecords - 운동 기록 관리",
        bodyRecords = "/api/bodyrecords - 신체 기록 관리",
        health = "/health - 헬스 체크",
        swagger = "/swagger - API 문서"
    }
});

app.UseHttpsRedirection();
app.MapControllers();

// 데이터베이스 연결 확인 (개발 환경에서만)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        try
        {
            // 데이터베이스 연결 확인
            await context.Database.CanConnectAsync();
            app.Logger.LogInformation("✅ 데이터베이스 연결이 성공적으로 확인되었습니다.");
            
            // 기본 데이터 확인
            var exerciseCount = await context.Exercises.CountAsync();
            var categoryCount = await context.Exercisecategories.CountAsync();
            app.Logger.LogInformation("📊 현재 DB 상태 - 운동: {ExerciseCount}개, 카테고리: {CategoryCount}개", 
                exerciseCount, categoryCount);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "❌ 데이터베이스 연결에 실패했습니다.");
        }
    }
}

app.Logger.LogInformation("🚀 FitnessPT API 서버가 시작되었습니다.");


app.Run();
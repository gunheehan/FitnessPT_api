using System.Reflection;
using System.Text.Json.Serialization;
using FitnessPT_api.Data;
using FitnessPT_api.GoogleAuth;
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// MySQL 연결
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


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
        Version = "v1",
        Description = @"
## 피트니스 PT 관리를 위한 REST API

### 주요 기능:
- **운동 관리**: 운동 CRUD, 카테고리별 분류, 난이도 설정
- **사용자 관리**: 사용자 계정, 프로필, 역할 관리
- **운동 기록**: 개인별 운동 기록 추적 및 분석
- **신체 기록**: 체중, 체지방률, 근육량 등 신체 데이터 관리

### 인증:
현재 버전은 인증이 없는 개발용입니다.

### 데이터베이스:
PostgreSQL 기반으로 구축되었습니다.
        ",
        Contact = new OpenApiContact
        {
            Name = "FitnessPT Development Team",
            Email = "dev@fitnesspt.com",
            Url = new Uri("https://github.com/fitnesspt/api")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    // XML 주석 활성화
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // 태그 정의로 API 그룹화
    c.TagActionsBy(api =>
    {
        if (api.ActionDescriptor.RouteValues["controller"] != null)
        {
            var controller = api.ActionDescriptor.RouteValues["controller"];
            return new[] { GetControllerDisplayName(controller) };
        }
        return new[] { "Default" };
    });

    // 스키마 ID 충돌 해결
    c.CustomSchemaIds(type => type.FullName);

    // 열거형을 문자열로 표시
    c.UseInlineDefinitionsForEnums();

    // 기본 응답 코드 문서화
    c.OperationFilter<DefaultResponsesOperationFilter>();

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

// 컨트롤러 목록 확인 엔드포인트
app.MapGet("/api/controllers", () =>
{
    var controllers = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(type => type.IsSubclassOf(typeof(Microsoft.AspNetCore.Mvc.ControllerBase)))
        .Select(type => new
        {
            Name = type.Name,
            Namespace = type.Namespace,
            Actions = type.GetMethods()
                .Where(m => m.IsPublic && !m.IsSpecialName && m.DeclaringType == type)
                .Select(m => m.Name)
                .ToArray()
        })
        .ToArray();
        
    return Results.Ok(new { controllers, count = controllers.Length });
});


app.UseHttpsRedirection();
app.MapControllers();

app.Logger.LogInformation("🚀 FitnessPT API 서버가 시작되었습니다.");

app.Run();

// 헬퍼 메서드들
static string GetControllerDisplayName(string controllerName)
{
    return controllerName switch
    {
        "Exercises" => "🏃‍♂️ 운동 관리",
        "Categories" => "📂 카테고리 관리",
        "Users" => "👥 사용자 관리",
        "UserProfiles" => "👤 사용자 프로필",
        "WorkoutRecords" => "🏋️‍♂️ 운동 기록",
        "BodyRecords" => "📊 신체 기록",
        _ => controllerName
    };
}

// Swagger 기본 응답 필터
public class DefaultResponsesOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        // 공통 응답 코드 추가
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "잘못된 요청 - 입력 데이터 검증 실패"
            });
        }
        
        if (!operation.Responses.ContainsKey("404"))
        {
            operation.Responses.Add("404", new OpenApiResponse
            {
                Description = "리소스를 찾을 수 없음"
            });
        }
        
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "서버 내부 오류"
            });
        }
    }
}
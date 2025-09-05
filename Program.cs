using FitnessPT_api.GoogleAuth;
using FitnessPT_api.GoogleAuth.Common.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers 추가
builder.Services.AddControllers();

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
}

// 정적 파일 서빙 추가 ⭐
app.UseStaticFiles();

// 기본 페이지 라우팅 추가 ⭐
app.MapGet("/", () => Results.Redirect("/test-login.html"));
app.MapGet("/test", () => Results.Redirect("/test-login.html"));


app.UseHttpsRedirection();
app.MapControllers();

app.Run();
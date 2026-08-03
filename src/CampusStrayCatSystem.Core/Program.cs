using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// JWT 密钥仅允许来自环境变量或未入库本地配置（如 appsettings.Development.json）。
// 支持：Auth:JwtSecret / Auth__JwtSecret / AUTH_JWT_SECRET
var jwtSecret = builder.Configuration["Auth:JwtSecret"]
    ?? builder.Configuration["AUTH_JWT_SECRET"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "缺少 Auth:JwtSecret。请通过环境变量 Auth__JwtSecret（或 AUTH_JWT_SECRET）或未提交的 appsettings.Development.json 配置，禁止使用仓库内固定密钥。");
}

var jwtIssuer = builder.Configuration["Auth:Issuer"] ?? "CampusStrayCatSystem";
var jwtAudience = builder.Configuration["Auth:Audience"] ?? "CampusStrayCatSystemClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<CampusStrayCatSystem.Models.User>, PasswordHasher<CampusStrayCatSystem.Models.User>>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SchemaFilter<Utf8ByteLengthSchemaFilter>());

// Dependency Injection - Repository
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IMedReminderRepository, MedReminderRepository>();
builder.Services.AddScoped<IEmergencyReportRepository, EmergencyReportRepository>();
builder.Services.AddScoped<IMissingAlertRepository, MissingAlertRepository>();
builder.Services.AddScoped<ITnrCaseRepository, TnrCaseRepository>();
builder.Services.AddScoped<ITnrStatusLogRepository, TnrStatusLogRepository>();
builder.Services.AddScoped<IMedHealthRecordRepository, MedHealthRecordRepository>();
builder.Services.AddScoped<ICatRepository, CatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserBlacklistRepository, UserBlacklistRepository>();
builder.Services.AddScoped<ICampusAreaRepository, CampusAreaRepository>();
builder.Services.AddScoped<IServicePointRepository, ServicePointRepository>();
builder.Services.AddScoped<INestMaintenanceRecordRepository, NestMaintenanceRecordRepository>();
builder.Services.AddScoped<ICatSightingRepository, CatSightingRepository>();

// 功能点20：投喂记录与交接记录
builder.Services.AddScoped<IReferenceCheckRepository, ReferenceCheckRepository>();
builder.Services.AddScoped<IVolShiftRepository, VolShiftRepository>();
builder.Services.AddScoped<IVolCheckInRepository, VolCheckInRepository>();
builder.Services.AddScoped<IVolHandoverRepository, VolHandoverRepository>();

// 功能点21：众筹财务公示与统计报表
builder.Services.AddScoped<IFundCrowdfundingProjectRepository, FundCrowdfundingProjectRepository>();
builder.Services.AddScoped<IFundDonationRepository, FundDonationRepository>();
builder.Services.AddScoped<IFundExpenseRecordRepository, FundExpenseRecordRepository>();
builder.Services.AddScoped<IRptStatisticsSnapshotRepository, RptStatisticsSnapshotRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

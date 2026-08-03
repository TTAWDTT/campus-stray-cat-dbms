using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SchemaFilter<Utf8ByteLengthSchemaFilter>());

// Repository 注册集中放在这里，便于后续按业务模块继续扩展。
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IMedReminderRepository, MedReminderRepository>();
builder.Services.AddScoped<IEmergencyReportRepository, EmergencyReportRepository>();
builder.Services.AddScoped<IMissingAlertRepository, MissingAlertRepository>();
builder.Services.AddScoped<ITnrCaseRepository, TnrCaseRepository>();
builder.Services.AddScoped<ITnrStatusLogRepository, TnrStatusLogRepository>();
builder.Services.AddScoped<IMedHealthRecordRepository, MedHealthRecordRepository>();
builder.Services.AddScoped<ICatRepository, CatRepository>();
builder.Services.AddScoped<ICatPhotoRepository, CatPhotoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICampusAreaRepository, CampusAreaRepository>();
builder.Services.AddScoped<IAdoptionWorkflowRepository, AdoptionWorkflowRepository>();
builder.Services.AddScoped<IVolunteerWorkflowRepository, VolunteerWorkflowRepository>();
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
builder.Services.AddSingleton<ICatPhotoFileStorage, CatPhotoFileStorage>();
builder.Services.AddScoped<CatPhotoService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();}

var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(webRootPath);
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(webRootPath) });
app.UseAuthorization();
app.MapControllers();
app.Run();

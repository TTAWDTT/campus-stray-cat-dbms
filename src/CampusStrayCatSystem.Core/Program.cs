using CampusStrayCatSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection - Repository
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICampusAreaRepository, CampusAreaRepository>();
builder.Services.AddScoped<ITnrCaseRepository, TnrCaseRepository>();
builder.Services.AddScoped<ITnrStatusLogRepository, TnrStatusLogRepository>();
builder.Services.AddScoped<IMedHealthRecordRepository, MedHealthRecordRepository>();
builder.Services.AddScoped<ICatRepository, CatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

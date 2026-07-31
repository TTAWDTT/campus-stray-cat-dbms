using CampusStrayCatSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repository 注册集中放在这里，便于后续按业务模块继续扩展。
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICampusAreaRepository, CampusAreaRepository>();
builder.Services.AddScoped<IAdoptionWorkflowRepository, AdoptionWorkflowRepository>();
builder.Services.AddScoped<IVolunteerWorkflowRepository, VolunteerWorkflowRepository>();

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

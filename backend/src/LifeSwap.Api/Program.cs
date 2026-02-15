using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IRequestWorkflowService, RequestWorkflowService>();
builder.Services.Configure<TeamsNotificationOptions>(
    builder.Configuration.GetSection(TeamsNotificationOptions.SectionName));
builder.Services.AddHttpClient<ITeamsNotificationService, TeamsNotificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    // Keep existing local SQLite databases compatible when new columns are introduced.
    // EnsureCreated does not alter an already created schema, so we apply a safe additive update here.
    if (dbContext.Database.IsSqlite())
    {
        using var connection = dbContext.Database.GetDbConnection();
        connection.Open();

        using var tableInfoCommand = connection.CreateCommand();
        tableInfoCommand.CommandText = "PRAGMA table_info('TimeOffRequests');";

        var hasDepartmentCodeColumn = false;
        using (var reader = tableInfoCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                var columnName = reader.GetString(1);
                if (columnName == nameof(TimeOffRequest.DepartmentCode))
                {
                    hasDepartmentCodeColumn = true;
                    break;
                }
            }
        }

        if (!hasDepartmentCodeColumn)
        {
            dbContext.Database.ExecuteSqlRaw(
                "ALTER TABLE TimeOffRequests ADD COLUMN DepartmentCode TEXT NOT NULL DEFAULT 'UNASSIGNED';");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendDev");
app.MapControllers();

app.Run();

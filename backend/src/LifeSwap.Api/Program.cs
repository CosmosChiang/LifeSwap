using LifeSwap.Api.Data;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendDev");
app.MapControllers();

app.Run();

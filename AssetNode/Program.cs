using System.Text.Json.Serialization;
using AssetNode.Data;
using AssetNode.Extensions;
using AssetNode.Extesnions;
using AssetNode.Interface;
using AssetNode.Middleware;
using AssetNode.Services;
using AssetNode.Services.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom services registration
builder.Services.AddCustomServices();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<AssetDbContext>(option =>
   option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting))
   );

// ✅ Correct CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173", // React dev server port
            "http://localhost:5175", // If your frontend runs here
            "https://localhost:5173",
            "https://localhost:5175"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
    DbInitilaizer.Initilaize(db);
}

using var context = new AssetDbContext(
      new DbContextOptionsBuilder<AssetDbContext>()
        .UseSqlServer("Data Source=DESKTOP-7GIK05C;Initial Catalog=AssetDb;Integrated Security=True;Encrypt=False;Trust Server Certificate=True") // ya SQL Server connection
        .Options);
    
SeedAdmin.Initialize(context);

app.UseHttpsRedirection();

// ✅ Order matters: UseCors before Authorization
app.UseCors("AllowFrontend");

app.UseAuthorization();

// Custom middlewares
//app.UseRateLimitg();

app.MapControllers();

app.Run();

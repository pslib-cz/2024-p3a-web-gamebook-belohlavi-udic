using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Gamebook.Server.Constants;

var builder = WebApplication.CreateBuilder(args);

// Logování
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Logování do konzole (volitelné)
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day) // Logování do souboru
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<GamebookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddSingleton<IEmailSender<User>, EmailSender<User>>();
builder.Services.AddControllers();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policy.Admin, policy => policy.RequireRole(Gamebook.Server.Constants.Role.Admin));
    options.AddPolicy(Policy.Author, policy => policy.RequireRole(Gamebook.Server.Constants.Role.Author));
});
// Identita
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<GamebookDbContext>();
    //.AddDefaultTokenProviders();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapGroup("/api/account").MapIdentityApi<User>();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

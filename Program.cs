
using InventoryEnterpriseProject.Infrastructure.Data;
using InventoryEnterpriseProject.Infrastructure.Repositories;
using InventoryEnterpriseProject.Services;
using InventoryEnterpriseProject.Core.Interfaces;
using InventoryEnterpriseProject.Middleware;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Text;

//Logging 
var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "inventory enterprise log", "log-.txt");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

//MVC Config
builder.Services.AddControllersWithViews();

//Smart Database Connection
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
var externalDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(externalDbUrl))
{
    var databaseUri = new Uri(externalDbUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    
    // Resolve Host to IPv4 to prevent Render "Network Unreachable" IPv6 issues
    string host = databaseUri.Host;
    try {
        var addresses = System.Net.Dns.GetHostAddresses(host);
        var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        if (ipv4 != null) host = ipv4.ToString();
    } catch { /* fallback to original host */ }

    var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = host,
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = Npgsql.SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true
    };
    connString = npgsqlBuilder.ToString();
}

// Entity Framework Context
builder.Services.AddDbContext<AppDbContext>(options => {
    if (string.IsNullOrEmpty(connString)) {
        options.UseInMemoryDatabase("InventoryDb");
    } else {
        options.UseNpgsql(connString);
    }
});

//Repositories & Dependency Injection 
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

//Session Context 
builder.Services.AddHttpContextAccessor();

//Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

var app = builder.Build();

// Automatically apply database migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (!context.Database.IsInMemory()) {
            context.Database.Migrate();
        }
    } 
    catch (Exception ex) 
    {
        Log.Error(ex, "An error occurred while migrating the database.");
    }
}

//Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

//Logging
app.UseSerilogRequestLogging();

//Static Files
app.UseStaticFiles();
app.UseRouting();

//Authentication
app.UseAuthentication();
app.UseAuthorization();

//Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inventory}/{action=Index}/{id?}");

app.Run();

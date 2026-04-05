
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

// Helper: parse a postgresql:// URL into a Npgsql connection string, with IPv4 resolution
string ParseDatabaseUrl(string dbUrl)
{
    var databaseUri = new Uri(dbUrl);
    var userInfo = databaseUri.UserInfo.Split(new[] { ':' }, 2); // limit 2 to preserve colons in password
    
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
    return npgsqlBuilder.ToString();
}

// Priority: DATABASE_URL env var > DefaultConnection from appsettings
if (!string.IsNullOrEmpty(externalDbUrl))
{
    connString = ParseDatabaseUrl(externalDbUrl);
}
else if (!string.IsNullOrEmpty(connString) && connString.StartsWith("postgresql://"))
{
    // DefaultConnection is also a URL format — parse it the same way
    connString = ParseDatabaseUrl(connString);
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

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Policy;
using Updater.Helper;
using Updater.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

//Cookie AUTH
builder.Services.AddAuthentication("CookieAuthentication")
        .AddCookie("CookieAuthentication", config =>
        {
            config.Cookie.Name = "_Auth";
            config.LoginPath = "/Login/Index";
            config.AccessDeniedPath = "/Login/AccessDenied";
        });

//END Cookie AUTH

//EF
string connection = builder.Configuration.GetConnectionString("db");
string provider = builder.Configuration.GetValue<string>("DBProvider");
if (provider.Equals("MSSQL"))
    builder.Services.AddDbContext<AppDBContext>(opt => opt.UseSqlServer(connection));
else if (provider.Equals("MYSQL"))
    builder.Services.AddDbContext<AppDBContext>(opt => opt.UseMySql(connection, ServerVersion.AutoDetect(connection)));
else
    builder.Services.AddDbContext<AppDBContext>(opt => opt.UseSqlServer(connection));

//END EF

builder.WebHost.ConfigureLogging((ctx, logging) =>
{
    logging.AddEventLog(options =>
    {
        options.SourceName = "Atualizador-Log";
    });
});

bool useIIS = builder.Configuration.GetValue<bool>("UseIIS");
bool dev = builder.Configuration.GetValue<bool>("Development");
bool https = builder.Configuration.GetValue<bool>("UseHttpsRedirection");

if (useIIS)
    builder.WebHost.UseIISIntegration();

string baseSiteURL = builder.Configuration.GetValue<string>("BaseURL");

BaseURL.URL = baseSiteURL;

var app = builder.Build();
app.UseDeveloperExceptionPage();

if (https)
{
    app.UseForwardedHeaders();

    app.UseHttpsRedirection();
}

if (dev)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

//JWT
app.UseAuthentication();
app.UseAuthorization();
//END JWT

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
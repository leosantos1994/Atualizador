using Microsoft.EntityFrameworkCore;
using Updater.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
builder.Services.AddDbContext<AppDBContext>(opt => opt.UseSqlServer(connection));
//END EF

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

builder.WebHost.UseIISIntegration();

//JWT
app.UseAuthentication();
app.UseAuthorization();
//END JWT

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
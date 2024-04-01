using AspNetCore.ReCaptcha;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using YeshasviniWeb;
using YeshasviniWeb.Hubs;
using YeshasviniWeb.Models;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
var LconnectionString = builder.Configuration.GetConnectionString("LocalConnection");
builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(LconnectionString, op =>
        op.CommandTimeout(240).EnableRetryOnFailure()));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    //Enables immediate logout, after updating the user's stat.
    options.ValidationInterval = TimeSpan.Zero;
});
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(6);
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<DbInitializer>();

builder.Services.AddTransient<ISumukhaSettingsService, SumukhaSettingsService>();
builder.Services.AddScoped<IDataAccessService, DataAccessService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
builder.Services.AddSignalR();
builder.Services.ConfigureApplicationCookie(options =>
{
    //Location for your Custom Login Page
    options.LoginPath = "/Login/";
});

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NRAiBiAaIQQuGjN/V0d+Xk9CfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5WdEBiUHtacH1dQ2hc;Mgo+DSMBMAY9C3t2VFhiQldPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtRc0RqXXteeXxSRGA=");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseMigrationsEndPoint();
    app.UseItToSeedSqlServer();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<MessageHub>("/messagehub");
app.MapRazorPages();
app.Run();

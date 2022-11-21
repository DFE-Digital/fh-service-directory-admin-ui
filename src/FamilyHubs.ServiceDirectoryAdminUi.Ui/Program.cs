using FamilyHubs.ServiceDirectory.Shared.Helpers;
using FamilyHubs.ServiceDirectory.Shared.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using Microsoft.AspNetCore.Builder;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
//Application Insights
RegisterComponents(builder.Services, builder.Configuration);

// Add services to the container.
builder.AddClientServices();


builder.Services
    //.AddClientServices()
    .AddWebUIServices(builder.Configuration);

builder.Services.AddTransient<IViewModelToApiModelHelper, ViewModelToApiModelHelper>();

builder.Services.AddTransient<IRedisCache, RedisCache>();
builder.Services.AddTransient<IRedisCacheService, RedisCacheService>();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();
builder.Services.AddTransient<ITokenService, TokenService>();


// Add services to the container.
builder.Services.AddRazorPages();

//TODO - add readable page routes - e.g.
//builder.Services.AddRazorPages().AddRazorPagesOptions(
//  options =>
//  {
//      options.Conventions.AddPageRoute("/Index", "home");
//      options.Conventions.AddPageRoute("/CheckServiceDetails", "check-service-details");
//  }).AddSessionStateTempDataProvider();

// Add Session middleware
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800); //TODO - set time in config
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    //options.DefaultChallengeScheme = "oidc";
}).AddCookie("Cookies");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ServiceMaintainer", policy =>
                    policy.RequireAssertion(context =>
                                context.User.IsInRole("LAAdmin") ||
                                context.User.IsInRole("VCSAdmin")));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapRazorPages();

app.Run();

static void RegisterComponents(IServiceCollection builder, IConfiguration configuration)
{
    builder.AddApplicationInsights(configuration, "fh_service_directory_admin_ui");
}

public partial class Program { }


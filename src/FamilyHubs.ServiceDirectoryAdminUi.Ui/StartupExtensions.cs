﻿using System.IdentityModel.Tokens.Jwt;
using FamilyHubs.ServiceDirectory.Shared.Helpers;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Extensions;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Api;
using FamilyHubs.ServiceDirectoryAdminUi.Ui.Services.Dataupload;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;

namespace FamilyHubs.ServiceDirectoryAdminUi.Ui;

public static class StartupExtensions
{
    public static void ConfigureHost(this WebApplicationBuilder builder)
    {
        // ApplicationInsights
        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            var logLevelString = builder.Configuration["LogLevel"];

            var parsed = Enum.TryParse<LogEventLevel>(logLevelString, out var logLevel);

            loggerConfiguration.WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces,
                parsed ? logLevel : LogEventLevel.Warning);

            loggerConfiguration.WriteTo.Console(
                parsed ? logLevel : LogEventLevel.Warning);
        });
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationInsightsTelemetry();
        // Add services to the container.
        services.AddClientServices(configuration);

        services.AddWebUIServices(configuration);

        services.AddTransient<IViewModelToApiModelHelper, ViewModelToApiModelHelper>();

        services.AddTransient<IRedisCache, RedisCache>();
        services.AddTransient<IRedisCacheService, RedisCacheService>();
        services.AddTransient<AuthenticationDelegatingHandler>();
        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IDatauploadService, DatauploadService>();

        // Add services to the container.
        services.AddRazorPages();

        // Add Session middleware
        services.AddDistributedMemoryCache();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionTimeOutMinutes"));
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            //options.DefaultChallengeScheme = "oidc";
        }).AddCookie("Cookies");

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ServiceMaintainer", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("DfEAdmin") ||
                    context.User.IsInRole("LAAdmin") ||
                    context.User.IsInRole("VCSAdmin")));
        });

    }

    public static IServiceProvider ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

#if use_https
        app.UseHttpsRedirection();
#endif
        
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseSession();

        app.MapRazorPages();

        return app.Services;
    }
}

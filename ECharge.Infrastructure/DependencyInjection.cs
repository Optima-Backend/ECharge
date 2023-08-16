using System;
using System.Text;
using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Job.Interface;
using ECharge.Domain.JWT.Interface;
using ECharge.Infrastructure.Services.ChargePointActions;
using ECharge.Infrastructure.Services.CibPay.Service;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Infrastructure.Services.EVtrip;
using ECharge.Infrastructure.Services.JWT;
using ECharge.Infrastructure.Services.Quartz;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace ECharge.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DataContext>();
            services.AddScoped<IChargeSession, ChargeSession>();

            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            services.AddQuartzHostedService();

            services.AddQuartz(x =>
            {
                x.UseInMemoryStore();
                x.UseDefaultThreadPool(10);
            });

            services.AddSingleton<IJwtService, JwtService>();

            services.AddHttpClient<IChargePointApiClient, ChargePointApiClient>();

            services.AddScoped<ICibPayService, CibPayService>();

            services.AddScoped<IChargePointAction, ChargePointAction>();

            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
                };
            });


            return services;
        }

    }
}



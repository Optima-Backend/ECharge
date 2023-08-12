using System;
using System.Text;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.JWT.Interface;
using ECharge.Infrastructure.Services.CibPay.Service;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Infrastructure.Services.EVtrip;
using ECharge.Infrastructure.Services.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECharge.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IChargePointApiClient, ChargePointApiClient>();

            services.AddSingleton<IJwtService, JwtService>();

            services.AddScoped<IChargePointApiClient, ChargePointApiClient>();

            services.AddScoped<ICibPayService, CibPayService>();

          
            
            
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



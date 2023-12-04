using ECharge.Api.Logging;
using ECharge.Infrastructure;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .WriteTo.Console()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

        builder.Services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        builder.Services.AddCors(options =>
        {
            string[] origins = new string[] { "http://127.0.0.1", "http://127.0.0.1:5500" };

            options.AddPolicy("CorsPolicy",
                builder => builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());

        });

        builder.Services.AddInfrastructure(configuration);

        builder.Services.AddControllers();
        builder.Services.AddControllersWithViews();


        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },

                    Array.Empty<string>()
                }
            });

            c.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("00:00:00")
            });
        });

        builder.Services.AddHttpClient();

        var app = builder.Build();

        //app.MapHub<ChargerHub>("/chargerHub");

        app.UseMiddleware<LoggingMiddleware>();

        app.UseCors("CorsPolicy");

        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECharge API v1");
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

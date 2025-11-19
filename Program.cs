using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using WebDbFirst.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using WebDbFirst.Controllers;

namespace WebDbFirst
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            string logDbConnectionString = builder.Configuration.GetConnectionString("AdventureWorksLT2019") ?? throw new InvalidOperationException("Connection string 'LogDatabase' not found.");

            ColumnOptions seriLogNewColumn = new();
            seriLogNewColumn.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn{ColumnName ="NuovaColonnaOrloffo"}
            };

            MSSqlServerSinkOptions mSSqlServerSink = new()
            {
                TableName = "Logs",
                AutoCreateSqlTable = true
            };

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("D:\\Temp\\webdbfirst.log", rollingInterval: RollingInterval.Day)
                .WriteTo.MSSqlServer(
                    connectionString: logDbConnectionString,
                    sinkOptions: mSSqlServerSink,
                    columnOptions: seriLogNewColumn)
                .CreateLogger();

            Log.Information("Bootstrap application starting");

            var nlogger = NLog.LogManager.GetCurrentClassLogger();

            nlogger.Info("NLOG *********************************************************************");

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AdventureWorksLt2019Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AdventureWorksLT2019") ?? throw new InvalidOperationException("Connection string 'AdventureWorksLT2019' not found.")));

            builder.Services.AddCors(
                opts =>
                {
                    opts.AddPolicy("CorsPolicia",
                    build => build
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((hosts) => true));
                });

            // JWT Authentication
            //eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im9ybG9mZiIsIm5iZiI6MTcxMzc4NzQxMCwiZXhwIjoxNzEzNzg3NDcwL
            //    CJpYXQiOjE3MTM3ODc0MTAsImlzcyI6IlVubyIsImF1ZCI6IkR1ZSJ9._jCXEWeoIQpjZPexxhAUCvv6myYTttuKMp3z0dYnrhs

            JwtSettings jwtSettings = new();
            //builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);
            jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = System.Text.Encoding.ASCII.GetBytes(jwtSettings.SecretKey!);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        //ValidateLifetime=true,
                        ClockSkew = TimeSpan.FromSeconds(3),
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            builder.Services.AddAuthorization(opt =>
            {
                opt.AddPolicy("AdminPolicy", policy =>
                {
                    policy.RequireRole("Admin");
                });

                opt.AddPolicy("UserPolicy", policy =>
                {
                    policy.RequireRole("User", "Admin");
                });
            });

            builder.Services.Configure<BnbMDBConfig>(builder.Configuration.GetSection("BnbMDBConfig"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CorsPolicia");
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

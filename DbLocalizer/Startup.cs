using DbLocalizer.Utility;
using Entities;
using Entities.DAL;
using Entities.Interfaces;
using Entities.Plugins.TranslationManagement;
using Entities.Plugins.TranslationManagement.Smartling;
using Entities.Services;
using Entities.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace DbLocalizer
{
    public class Startup
    {
        readonly string CORSPolicy = "DbLocalizerCorsPolicy";

        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddSingleton<IEncryptionService, EncryptionService>();

            services.AddScoped<IGenericPluginInData, GenericPluginInData>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddSingleton<IDataProvider, DataProvider>();
            services.AddSingleton<ISqlSchemaBuilder, SqlSchemaBuilder>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddScoped<ISmartlingDataService, SmartlingDataService>();

            // configuration
            services.Configure<AppSettings>(_configuration);
            services.AddScoped<AppSettings>(_ => _configuration.Get<AppSettings>());
            services.AddSingleton<IBackgroundWorkerQueue, BackgroundWorkerQueue>();
            services.AddSingleton<ILongRunningService, LongRunningService>();
            services.AddHostedService<LongRunningService>(p => p.GetRequiredService<ILongRunningService>() as LongRunningService);
            services.AddHealthChecks().AddCheck<HealthChecker>("DbLocalizer");
            services.AddQuartz(q => ConfiguredScheduledJobs(q, _configuration));
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<IFileDataService, FileDataService>();
            services.AddScoped<IExportDal, ExportDal>();
            services.AddScoped<IImportDal, ImportDal>();
            services.AddScoped<ICacheManager, CacheManager>();

            services.AddScoped<ISmartlingConfiguration>(provider =>
            {
                return new SmartlingConfiguration(_configuration);
            });

            services.AddScoped<ISmartlingFileDataService, SmartlingFileDataService>();
            services.AddScoped<ISmartlingImportUtility, SmartlingImportUtility>();
            services.AddScoped<ISmartlingImportFileProcessor, SmartlingImportFileProcessor>();
            services.AddScoped<ISmartlingSqlProcessor, SmartlingSqlProcessor>();

            


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "DbLocalizer",
                    Description = "This service automates the import and export of database resource strings",
                    Contact = new OpenApiContact
                    {
                        Name = "DbLocalizer",
                        Email = "DbLocalizer@gmail.com"
                    }
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your token in the text input below.\nExample: Bearer <your_token>"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            var cors = _configuration.Get<AppSettings>().Cors.CorsOrigins;
            services.AddCors(options => options.AddPolicy(CORSPolicy, builder =>
            {
                builder.WithOrigins(cors)
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // Add services to the container.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "DbLocalizer", // Replace with your actual issuer
                        ValidateAudience = true,
                        ValidAudience = "DbLocalizer", // Replace with your actual audience
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = System.TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecureKeysManager.SecureKeys?["jwtSecretKey"].Trim())),
                    };
                });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseRouting();
            // USE CORS
            app.UseCors(CORSPolicy);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = HealthCheckExtensions.WriteResponse
                });
            });

        }

        private static void ConfiguredScheduledJobs(IServiceCollectionQuartzConfigurator q, IConfiguration config)
        {
            //only run these jobs if scheduler is explicitly specified
            if (config.GetValue<Boolean>("UseScheduler"))
            {
                // default export
                q.ScheduleJob<ExportScheduler>(
                    trigger => trigger
                        .WithIdentity("exportTrigger", "exportGroup")
                        .WithCronSchedule(config.GetValue<string>("ScheduledExportCron")),
                    job => job
                        .WithIdentity("exportJob", "exportGroup")
                        .UsingJobData("exportEndpoint", config.GetValue<string>("ExportEndPoint")));
            }
        }
    }
}

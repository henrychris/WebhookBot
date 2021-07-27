using System;
using System.IO;
using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NLog;
using Telegram.Bot;
using app.Components;
using app.Services;
using app.Data;
using Microsoft.EntityFrameworkCore;
using app.Interfaces;
using app.Data.Repository;

namespace app
{

    public static class ServicesExtensions
    {

        public static void AddComponents(this IServiceCollection services)
        {
            services.AddTransient<Keyboards>();
            services.AddScoped<ErrorHandler>();
        }

        public static void ConfigureAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
        }
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureDbContext(this IServiceCollection services, string dbConnectionString)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(dbConnectionString);
            });
        }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            BotConfig = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
        }
        public IConfiguration Configuration { get; }
        protected BotConfiguration BotConfig { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "app v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors((builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                var token = BotConfig.BotToken;
                endpoints.MapControllers();

                endpoints.MapControllerRoute(name: "tgwebhook",
                                            pattern: $"bot/{token}",
                                            new { controller = "Webhook", action = "Post" });
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Telegram.Bot uses JSON.NET to deserialize incoming messages.
            services.AddControllers().AddNewtonsoftJson();

            services.AddHostedService<ConfigureWebhook>();

            services.AddScoped<HandleUpdateService>();

            services.AddHttpClient("tgwebhook")
                    .AddTypedClient<ITelegramBotClient>(httpClient
                        => new TelegramBotClient(BotConfig.BotToken, httpClient));

            services.AddScoped<IUserRepository, UserRepository>();

            services.ConfigureLoggerService();
            services.ConfigureAutoMapper();
            services.ConfigureDbContext(BotConfig.DefaultConnection);
            
            services.AddComponents();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "app", Version = "v1" });
            });
        }
    }
}

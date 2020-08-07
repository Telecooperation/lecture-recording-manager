using LectureRecordingManager.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using System;
using LectureRecordingManager.Jobs;
using RecordingProcessor.Studio;
using Microsoft.AspNetCore.Http.Features;
using LectureRecordingManager.Hubs;
using Newtonsoft.Json;

namespace LectureRecordingManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc; // this should be set if you always expect UTC dates in method bodies, if not, you can use RoundTrip instead.
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddHangfire(config => config.UsePostgreSqlStorage(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // add media convertion utils
            services.AddTransient<ChromaKeyParamGuesser, ChromaKeyParamGuesser>();
            services.AddTransient<MediaConverter, MediaConverter>();

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = long.MaxValue;
            });

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            // Configure hangfire to use the new JobActivator we defined.
            GlobalConfiguration.Configuration
                .UseActivator(new ContainerJobActivator(serviceProvider));

            var options = new BackgroundJobServerOptions { WorkerCount = Environment.ProcessorCount };
            app.UseHangfireServer(options);

            app.UseHangfireDashboard();

            // init background
            RecurringJob.AddOrUpdate<ScheduledPublishingRecordingJob>("scheduled-publishing-task", x => x.CheckPublishingRecordings(), Cron.MinuteInterval(5));

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<MessageHub>("/messagehub");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}

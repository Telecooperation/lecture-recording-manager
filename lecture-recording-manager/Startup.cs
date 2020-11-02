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
using Hangfire.Dashboard;
using LectureRecordingManager.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Web;

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
            services.AddControllers()
                .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddHangfire(config => config.UsePostgreSqlStorage(Configuration.GetConnectionString("DefaultConnection"), new PostgreSqlStorageOptions()
            {
                InvisibilityTimeout = new TimeSpan(24, 0, 0)
            }));

            services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // For Identity  
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DatabaseContext>()
                .AddDefaultTokenProviders();

            // Adding Authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

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
            GlobalConfiguration.Configuration.UseActivator(new ContainerJobActivator(serviceProvider));

            var options = new BackgroundJobServerOptions { WorkerCount = Environment.ProcessorCount };
            app.UseHangfireServer(options);

            app.UseHangfireDashboard();

            // init background
            RecurringJob.AddOrUpdate<ScheduledPublishingRecordingJob>("scheduled-publishing-task", x => x.CheckPublishingRecordings(), Cron.MinuteInterval(5));
            RecurringJob.AddOrUpdate<ScheduledScanRecordingsJob>("scheduled-scan-task", x => x.ScanRecordings(), Cron.MinuteInterval(5));

            // init db
            UpdateDatabase(app);

            // query param auth
            app.Use(async (context, next) =>
            {
                if (context.Request.QueryString.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                    {
                        var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
                        string token = queryString.Get("access_token");

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", new[] { string.Format("Bearer {0}", token) });
                        }
                    }
                }

                await next.Invoke();
            });

            // init routing
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DatabaseContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}

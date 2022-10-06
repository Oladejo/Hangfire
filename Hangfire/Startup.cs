using Hangfire.AppDbContext;
using Hangfire.Services;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace Hangfire
{
    public class Startup
    {
        private static readonly IEmployeeService employeeService;
        private readonly Job jobscheduler = new Job(employeeService);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hangfire", Version = "v1" });
            });

            #region Configure Connection String  
            services.AddDbContext<EmployeeDbContext>(item => item.UseSqlServer(Configuration.GetConnectionString("Default")));
            #endregion

            #region Configure Hangfire  
            services.AddHangfire(c => c.UseSqlServerStorage(Configuration.GetConnectionString("Default")));
            GlobalConfiguration.Configuration.UseSqlServerStorage(Configuration.GetConnectionString("Default")).WithJobExpirationTimeout(TimeSpan.FromDays(7));
            #endregion

            #region Services Injection  
            services.AddTransient<IEmployeeService, EmployeeService>();
            #endregion


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hangfire v1"));
            }


            #region Configure Hangfire
            app.UseHangfireServer();

            //Basic Authentication added to access the Hangfire Dashboard
            //app.UseHangfireDashboard("/dashboard", new DashboardOptions()
            //{
            //    AppPath = null,
            //    DashboardTitle = "Hangfire Dashboard",
            //    Authorization = new[]{
            //        new HangfireCustomBasicAuthenticationFilter{
            //            User = Configuration.GetSection("HangfireCredentials:UserName").Value,
            //            Pass = Configuration.GetSection("HangfireCredentials:Password").Value
            //        }
            //        //new HangfireAuthorizationFilter()
            //},
            //    //Authorization = new[] { new DashboardNoAuthorizationFilter() },
            //    //IgnoreAntiforgeryToken = true
            //});

            app.UseHangfireDashboard("/dashboard", new DashboardOptions()
            {
                Authorization = new[] {new HangfireAuthorizationFilter() }
            });
            #endregion

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region job scheduling tasks
            // recurring job for every 5 min
            recurringJobManager.AddOrUpdate("insert employee : runs every 1 min", () => jobscheduler.JobAsync(), "*/5 * * * *");

            //fire and forget job 
            var jobid = backgroundJobClient.Enqueue(() => jobscheduler.JobAsync());

            //continous job
            backgroundJobClient.ContinueJobWith(jobid, () => jobscheduler.JobAsync());

            //schedule job / delayed job

            backgroundJobClient.Schedule(() => jobscheduler.JobAsync(), TimeSpan.FromDays(5));
            #endregion
        }
    }
}

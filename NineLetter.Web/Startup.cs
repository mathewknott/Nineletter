using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NineLetter.Web.Data;
using NineLetter.Web.Models;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace NineLetter.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
            
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            var appConfig = new NineLetterConfig();
            Configuration.GetSection("NineLetter").Bind(appConfig);

            _hostingEnvironment = env;

        }

        public IConfigurationRoot Configuration { get; }

        private IHostingEnvironment _hostingEnvironment;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc();

            services.AddSwaggerGen();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Nine Letter Game API",
                    Description = "Nine Letter Game",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Mathew Knott", Email = "", Url = "http://mathewknott.com" }
                });
            });

            services.ConfigureSwaggerGen(options =>
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "NineLetter.Web.xml");
                options.IncludeXmlComments(xmlPath);
            });


            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;
            services.AddSingleton(physicalProvider);

            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.CookieName = ".NineLetter";
            });
                        
            services.Configure<NineLetterConfig>(
                Configuration.GetSection("NineLetter"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
                        
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
            }
            
            app.UseStaticFiles();

            app.UseSession();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Acme Fun Events API");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "errorCode",
                    "error/{statusCode}",
                    new
                    {
                        controller = "Home",
                        action = "Error",
                    });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

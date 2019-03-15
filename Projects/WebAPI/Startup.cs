using JayCoder.MusicStore.Core.Domain.Constants;
using JayCoder.MusicStore.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace JayCoder.MusicStore.Projects.WebAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var svcBuilder = services
                .AddMvcCore()
                .AddApiExplorer()
                .AddJsonFormatters();

            svcBuilder = svcBuilder.AddAuthorization(
                options => BuildAuthorizationPolicies(options)
            );

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "MusicStore API", Version = "v1" });
            });

            services
                .AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration.GetValue<string>("Authority");
                    options.RequireHttpsMetadata = Configuration.GetValue<bool>("RequireHttpsMetadata");
                    options.ApiName = Configuration.GetValue<string>("ApiName");
                });

            string strOrigionList = Configuration.GetValue<string>("AllowedOrigions");
            if (!string.IsNullOrEmpty(strOrigionList))
            {
                services.AddCors(options =>
                {
                    // this defines a CORS policy called "default"
                    options.AddPolicy("default", policy =>
                    {
                        policy.WithOrigins(strOrigionList.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("default");
            app.UseAuthentication();

            app.UseMvc();

            // Use Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicStore API V1");
            });
        }

        private void BuildAuthorizationPolicies(AuthorizationOptions options)
        {
            //Basic Policies
            foreach (var userType in Enum.GetNames(typeof(EnumUserType)))
            {
                options.AddPolicy(userType, policy => policy.RequireClaim(AuthorizeClaim.UserType, userType));
            }
        }
    }
}
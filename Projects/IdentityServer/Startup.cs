using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using JayCoder.MusicStore.Core.Domain.SQLEntities;
using JayCoder.MusicStore.Foundations.SQLServer.AspNetIdentity;
using JayCoder.MusicStore.Projects.IdentityServer.Configuration;
using JayCoder.MusicStore.Projects.IdentityServer.Profile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace JayCoder.MusicStore.Projects.IdentityServer
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var connectionString = Configuration.GetConnectionString("IdentityServer4Connection");
            // Init DbContext ConnectionString
            var aspnetIdentityAssembly = typeof(AspNetIdentityDbContext).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<AspNetIdentityDbContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(aspnetIdentityAssembly))
            );
            // Apply AspNetIdentity as default token provider
            // UserManager can be used to manage users
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AspNetIdentityDbContext>()
            .AddDefaultTokenProviders();
            // Load resources from DB
            var identityServerAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var identityServer = services
                .AddIdentityServer()
                .AddAspNetIdentity<ApplicationUser>()
                // this adds the config data from DB (clients, resources, CORS)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(identityServerAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(identityServerAssembly));
                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 15; // interval in seconds. 15 seconds useful for debugging
                });

            services.AddAuthentication()
                .AddLinkedIn(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["LinkedIn:ClientId"];
                    options.ClientSecret = Configuration["LinkedIn:ClientSecret"];

                    options.CallbackPath = new PathString("/linkedin");
                    options.SaveTokens = true;
                })
                .AddMicrosoftAccount(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = Configuration["Microsoft:ApplicationId"];
                    options.ClientSecret = Configuration["Microsoft:Password"];

                    options.CallbackPath = new PathString("/microsoft");
                    options.SaveTokens = true;
                });

            if (Environment.IsDevelopment())
            {
                identityServer.AddDeveloperSigningCredential();
            }

            services.AddTransient<IProfileService, ProfileService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Init Database using EF Code First
            InitializeDatabase(app);

            app.UseStaticFiles();

            app.UseIdentityServer();
            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                //PersistedGrant DB Context
                var persistedGrantContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                persistedGrantContext.Database.Migrate();
                //Configuration DB Context
                var configContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configContext.Database.Migrate();
                if (!configContext.Clients.Any())
                {
                    var clientList = new PortalClientFactory(Configuration).GetClients();
                    if (clientList != null && clientList.Any())
                    {
                        foreach (var client in clientList)
                        {
                            configContext.Clients.Add(client.ToEntity());
                        }
                        configContext.SaveChanges();
                    }
                }

                if (!configContext.IdentityResources.Any())
                {
                    foreach (var resource in Resources.GetIdentityResources())
                    {
                        configContext.IdentityResources.Add(resource.ToEntity());
                    }
                    configContext.SaveChanges();
                }

                if (!configContext.ApiResources.Any())
                {
                    foreach (var resource in Resources.GetApiResources())
                    {
                        configContext.ApiResources.Add(resource.ToEntity());
                    }
                    configContext.SaveChanges();
                }

                // AspNet Identity DB Context
                var idContext = serviceScope.ServiceProvider.GetRequiredService<AspNetIdentityDbContext>();
                if (!idContext.Users.Any())
                {
                    var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                    foreach (var user in Resources.GetApplicationUsers())
                    {
                        var result1 = userManager.CreateAsync(user , "Test123!").Result;
                    }
                }
                idContext.Database.Migrate();
            }
        }
    }
}

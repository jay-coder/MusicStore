using JayCoder.MusicStore.Core.Domain.SQLEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace JayCoder.MusicStore.Foundations.SQLServer.AspNetIdentity
{
    public class AspNetIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        private string ConnectionString { get; set; }
        //Need Parameterless Constructor for EF Migration
        public AspNetIdentityDbContext()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                string strAppSettings = "appsettings.json";
                if (!string.IsNullOrEmpty(env))
                {
                    strAppSettings = string.Format("appsettings.{0}.json", env);
                }
                // EF Code First
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(strAppSettings)
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("IdentityServer4Connection"));
            }
            else
            {
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }

        public AspNetIdentityDbContext(DbContextOptions<AspNetIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
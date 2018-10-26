using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.eShopWeb.Infrastructure.Data;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)
                        .Build();

            using (var scope = host.Services.CreateScope())
            {

                var scopedServices = scope.ServiceProvider;

                // Catalog
                var catalogDB = scopedServices.GetRequiredService<CatalogContext>();

                // Ensure the database is created.
                catalogDB.Database.EnsureCreated();
                catalogDB.Database.Migrate();

                var loggerFactory = scopedServices.GetRequiredService<ILoggerFactory>();

                try
                {
                    // Seed the database with test data.
                    CatalogContextSeed.SeedAsync(catalogDB, loggerFactory).Wait();
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogError(ex, "An error occurred while migrating the database. Catalog");
                }

                ////// Identity
                ////var identityDB = scopedServices.GetRequiredService<AppIdentityDbContext>();

                ////// Ensure the database is created.
                ////identityDB.Database.EnsureCreated();
                ////identityDB.Database.Migrate();


                ////try
                ////{
                ////    // Seed the database with test data.
                ////    var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                ////    AppIdentityDbContextSeed.SeedAsync(userManager).Wait();
                ////}
                ////catch (Exception ex)
                ////{
                ////    var logger = scopedServices.GetRequiredService<ILogger<Startup>>();
                ////    logger.LogError(ex, "An error occurred while migrating the database. Identity");
                ////}

                ////    var services = scope.ServiceProvider;
                ////    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                ////    try
                ////    {
                ////        var catalogContext = services.GetRequiredService<CatalogContext>();
                ////        CatalogContextSeed.SeedAsync(catalogContext, loggerFactory)
                ////.Wait();

                ////        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                ////        AppIdentityDbContextSeed.SeedAsync(userManager).Wait();
                ////    }
                ////    catch (Exception ex)
                ////    {
                ////        var logger = loggerFactory.CreateLogger<Program>();
                ////        logger.LogError(ex, "An error occurred seeding the DB.");
                ////    }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseCloudFoundryHosting()
                .UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build())
                .UseCloudFoundryHosting()
                .AddCloudFoundry()
                //.UseUrls("http://0.0.0.0:5106")
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    var env = builderContext.HostingEnvironment;
                    configBuilder.SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        // Add to configuration the Cloudfoundry VCAP settings
                        .AddCloudFoundry();
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                });
    }
}

using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Infrastructure.Logging;
using Microsoft.eShopWeb.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.CloudFoundry.Connector.SqlServer.EFCore;

namespace Microsoft.eShopWeb.Web
{
    public class Startup
    {
        private IServiceCollection _services;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            // use in-memory database
            //ConfigureInMemoryDatabases(services);

            // use real database
            ConfigureProductionServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            services.AddOptions();
            services.ConfigureCloudFoundryOptions(Configuration);

            // use real database
            // Requires LocalDB which can be installed with SQL Server Express 2016
            // https://www.microsoft.com/en-us/download/details.aspx?id=54284

            //services.AddDbContext<CatalogContext>(c =>
            //c.UseSqlServer(Configuration["vcap:application:catalog-db-service:credentials:connection"]));

            services.AddDbContext<CatalogContext>(options => options.UseSqlServer(Configuration));

            // Add Identity DbContext
            //services.AddDbContext<AppIdentityDbContext>(options =>
            //    options.UseSqlServer(Configuration["vcap:application:identity-db-service:credentials:connection"]));

            //////services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(Configuration));

            /*
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
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

                // Identity
                var identityDB = scopedServices.GetRequiredService<AppIdentityDbContext>();

                // Ensure the database is created.
                identityDB.Database.EnsureCreated();
                identityDB.Database.Migrate();


                try
                {
                    // Seed the database with test data.
                    var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                    AppIdentityDbContextSeed.SeedAsync(userManager).Wait();
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogError(ex, "An error occurred while migrating the database. Identity");
                }

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
            
             */

            ////// Build the service provider.
            ////var sp = services.BuildServiceProvider();

            ////// Create a scope to obtain a reference to the database
            ////// context (ApplicationDbContext).
            ////using (var scope = sp.CreateScope())
            ////{
            ////    var scopedServices = scope.ServiceProvider;

            ////    // Catalog
            ////    var catalogDB = scopedServices.GetRequiredService<CatalogContext>();

            ////    // Ensure the database is created.
            ////    catalogDB.Database.EnsureCreated();
            ////    catalogDB.Database.Migrate();

            ////    var loggerFactory = scopedServices.GetRequiredService<ILoggerFactory>();

            ////    try
            ////    {
            ////        // Seed the database with test data.
            ////        CatalogContextSeed.SeedAsync(catalogDB, loggerFactory).Wait();
            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        var logger = scopedServices.GetRequiredService<ILogger<Startup>>();
            ////        logger.LogError(ex, "An error occurred while migrating the database. Catalog");
            ////    }

            ////    // Identity
            ////    var identityDB = scopedServices.GetRequiredService<CatalogContext>();

            ////    // Ensure the database is created.
            ////    identityDB.Database.EnsureCreated();
            ////    identityDB.Database.Migrate();

            ////    try
            ////    {
            ////        // Seed the database with test data.
            ////        CatalogContextSeed.SeedAsync(identityDB, loggerFactory).Wait();
            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        var logger = scopedServices.GetRequiredService<ILogger<Startup>>();
            ////        logger.LogError(ex, "An error occurred while migrating the database. Identity");
            ////    }
            ////}


            ConfigureServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            
            //////services.AddIdentity<ApplicationUser, IdentityRole>()
            //////    .AddEntityFrameworkStores<AppIdentityDbContext>()
            //////    .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = "/Account/Signin";
                options.LogoutPath = "/Account/Signout";
                options.Cookie = new CookieBuilder
                {
                    IsEssential = true // required for auth to work without explicit user conclssent; adjust to suit your privacy policy
                };
            });

           
            //services.AddDbContext<CatalogContext>(c => c.UseInMemoryDatabase("Catalog"));
            //services.AddDbContext<AppIdentityDbContext>(c => c.UseInMemoryDatabase("Identity"));

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));
            
            services.AddScoped<ICatalogService, CachedCatalogService>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IBasketViewModelService, BasketViewModelService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<CatalogService>();
            services.Configure<CatalogSettings>(Configuration);
            services.AddSingleton<IUriComposer>(new UriComposer(Configuration.Get<CatalogSettings>()));

            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc()
                .SetCompatibilityVersion(AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            _services = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                ListAllRegisteredServices(app);
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Catalog/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc();

            //using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            //{
            //    var catContext = serviceScope.ServiceProvider.GetRequiredService<CatalogContext>();
            //    catContext.Database.Migrate();

            //    var identityContext = serviceScope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
            //    identityContext.Database.Migrate();
            //}

        }

        private void ListAllRegisteredServices(IApplicationBuilder app)
        {
            app.Map("/allservices", builder => builder.Run(async context =>
            {
                var sb = new StringBuilder();
                sb.Append("<h1>All Services</h1>");
                sb.Append("<table><thead>");
                sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
                sb.Append("</thead><tbody>");
                foreach (var svc in _services)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{svc.ServiceType.FullName}</td>");
                    sb.Append($"<td>{svc.Lifetime}</td>");
                    sb.Append($"<td>{svc.ImplementationType?.FullName}</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
                await context.Response.WriteAsync(sb.ToString());
            }));
        }
    }
}

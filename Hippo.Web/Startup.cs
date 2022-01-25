using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

namespace Hippo.Web
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
            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            // TODO: authentication
            
            // TODO: database/EF

            // TODO: authorization

            // TODO: config

            // TODO: DI

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // TODO: DB config/init

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
            app.UseSpaStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    if (context.Context.Request.Path.StartsWithSegments("/static"))
                    {
                        var headers = context.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(365)
                        };
                    }
                }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // default for MVC server-side endpoints
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new { controller = "(home)" }
                );

                // TODO: API routes map to all other controllers

                // any other nonfile route should be handled by the spa, except leave the sockjs route alone if we are in dev mode (hot reloading)
                if (env.IsDevelopment()) {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" },
                        constraints: new { path = new RegexRouteConstraint("^(?!sockjs-node).*$") }
                    );
                } else {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" }
                    );
                }
            });

            // SPA needs to kick in for all paths during development
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
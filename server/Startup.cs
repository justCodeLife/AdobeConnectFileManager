using System.IO;
using adobe_manager.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace adobe_manager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSignalR();
            services.AddCors(options =>
            {
                options.AddPolicy("CORS",
                    builder =>
                    {
                        builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CORS");
            app.UseRouting();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "public")),
                RequestPath = ""
            });
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/",
                    async context => { await context.Response.SendFileAsync("public/index.html"); });
                endpoints.MapHub<AdobeHub>("/adobe", o => { o.Transports = HttpTransportType.WebSockets; });
            });
        }
    }
}
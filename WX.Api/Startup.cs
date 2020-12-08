using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WX.Api.Abstractions;
using WX.Api.Models;
using WX.Api.Services;

namespace WX.Api
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
            services.AddHttpClient();

            services.AddSingleton<ISerializer, Serializer>();
            services.AddSingleton<ISettings, Settings>();
            services.AddSingleton<IResourceService, ResourceService>();

            services.AddControllers()
                .AddJsonOptions(options => 
                    Serializer.SetJsonSerializerOptions(options.JsonSerializerOptions));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
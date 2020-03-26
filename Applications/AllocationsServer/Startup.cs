﻿using System;
using System.Net.Http;
using Allocations.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Management.CloudFoundry;

namespace AllocationsServer
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
            services.AddCloudFoundryActuators(Configuration);

            services.AddControllers();
            
            services.AddDiscoveryClient(Configuration);

            services.AddScoped<IAllocationDataGateway, AllocationDataGateway>();
            services.AddDbContext<AllocationContext>(options => options.UseMySql(Configuration));
            
            services.AddSingleton<Allocations.ProjectClient.IProjectClient>(sp =>
            {
                var handler = new DiscoveryHttpClientHandler(sp.GetService<IDiscoveryClient>());
                var httpClient = new HttpClient(handler, false)
                {
                    BaseAddress = new Uri(Configuration.GetValue<string>("REGISTRATION_SERVER_ENDPOINT"))
                };

                return new Allocations.ProjectClient.ProjectClient(httpClient);
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCloudFoundryActuators();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseDiscoveryClient();
        }
    }
}
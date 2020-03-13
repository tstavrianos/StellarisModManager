﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace StellarisModManager.Blazor
{
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/Error");
            
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");
                });
        }
    }
}

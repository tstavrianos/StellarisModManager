namespace StellarisModManager.Cross
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;


    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if DEBUG
            app.UseExceptionHandler(
                    errorApp =>
                        {
                            errorApp.Run(
                                async context =>
                                    {
                                        context.Response.StatusCode = 500;
                                        context.Response.ContentType = "text/html";
                                        await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
                                        await context.Response.WriteAsync("ERROR!<br><br>\r\n");

                                        var exceptionHandlerPathFeature =
                                            context.Features.Get<IExceptionHandlerPathFeature>();

                                        if (exceptionHandlerPathFeature != null)
                                        {
                                            // Get which route the exception occurred at
                                            string routeWhereExceptionOccurred = exceptionHandlerPathFeature.Path;

                                            // Get the exception that occurred
                                            Exception exceptionThatOccurred = exceptionHandlerPathFeature.Error;
                                            Log.Error(exceptionThatOccurred, routeWhereExceptionOccurred);

                                        }

                                        await context.Response.WriteAsync("<a href=\"/\">Home</a><br>\r\n");
                                        await context.Response.WriteAsync("</body></html>\r\n");
                                        await context.Response.WriteAsync(new string(' ', 512)); // IE padding

                                    });
                        });
#else
            app.UseExceptionHandler("/Error");
#endif

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

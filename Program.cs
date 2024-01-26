using BlazorApp1.Components;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;

namespace BlazorApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Set the Kestrel webserver to have longer timeouts.
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                serverOptions.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(30);
                serverOptions.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromMinutes(1);
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();



            app.UsePathBase(new PathString("/blazor/"));
            app.UseRouting();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.Use((context, next) =>
            {
                //The following line adds the siteroot back to the url when being returned to the client, such that https://decumaria:5000/mypage.html becomes https://decumaria:5000:/siteroot/mypage.html
                context.Request.PathBase = new PathString("/blazor");
                context.Request.Scheme = "https";
                //WavaFix: Missing or insecure "X-Content-Type-Options" header
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                //WavaFix:
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                //WavaFix: Only allow iframes from the same origin
                context.Response.Headers.Append("X-Frame-Options", "sameorigin");
                // New locations for powerpoints, pdfs, etc, need to be added the frame-src and connect-src accordingly.
                //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; object-src 'self'; style-src 'self'; img-src 'self'; media-src 'self'; frame-ancestors 'self'; frame-src 'self'; connect-src 'self';");
                return next();
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}

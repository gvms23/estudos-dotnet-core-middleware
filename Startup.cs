using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Mime;
using Utf8Json;

namespace EstudosMiddleware
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(
                new ExceptionHandlerOptions
                {
                    ExceptionHandler = async context =>
                    {
                        var _exceptionThrown = context.Features.Get<IExceptionHandlerPathFeature>();

                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = MediaTypeNames.Application.Json;

                        await context.Response.WriteAsync(
                            JsonSerializer.ToJsonString(
                                new
                                {
                                    Code = "XPTO",
                                    HttpStatusCode = context.Response.StatusCode,
                                    Message = _exceptionThrown.Error.Message.ToString(),
                                    Details = env.IsDevelopment()
                                        ? JsonSerializer.ToJsonString(_exceptionThrown.Error)
                                        : null
                                }));

                    }
                });

            app.Use(async (context, next) =>
            {
                throw new Exception("test", new InvalidOperationException());
                await context.Response.WriteAsync("\n1st delegate begins");
                await next();
                await context.Response.WriteAsync("\n1st delegate ends");
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync("\n2nd delegate called");
                //await next()
            });

            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("\n3rd delegate begins");
                await next();
                await context.Response.WriteAsync("\n3rd delegate ends");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using CoreIdentity.API.Helpers;
using CoreIdentity.API.Identity;
using CoreIdentity.API.Middleware;
using CoreIdentity.API.Services;
using CoreIdentity.API.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreIdentity.API
{
    // ref #1: https://logcorner.com/token-based-authentication-using-asp-net-web-api-core/
    // ref #2: https://social.technet.microsoft.com/wiki/contents/articles/36804.asp-net-core-mvc-authentication-and-role-based-authorization-with-asp-net-core-identity.aspx#Add_Edit_Application_Role
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
            // Identity
            services.AddDbContext<SecurityContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:Default"]));

            services.AddHttpContextAccessor();

            // Tools->NuGet Package Manager -> Package Manager Console
            // Initialise
            // add-migration init -Context SecurityContext
            // update or create DB
            // update-database -Context SecurityContext
            IdentityHelper.ConfigureService(services);

            // Helpers
            AuthenticationHelper.ConfigureService(services, Configuration["JwtSecurityToken:Issuer"], Configuration["JwtSecurityToken:Audience"], Configuration["JwtSecurityToken:Key"]);
            CorsHelper.ConfigureService(services);
            SwaggerHelper.ConfigureService(services);

            // Settings
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<ClientAppSettings>(Configuration.GetSection("ClientApp"));
            services.Configure<JwtSecurityTokenSettings>(Configuration.GetSection("JwtSecurityToken"));
            services.Configure<QRCodeSettings>(Configuration.GetSection("QRCode"));

            // Services
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IRefreshTokenService, RefreshTokenService>();
            
            //// Azure
            //// Azure Storage Services
            //services.AddScoped<IBlobStorage>(s => new BlobStorage(Configuration["ConnectionStrings:AzureStorage"], Configuration["AzureStorage:ContainerName"], Configuration["AzureStorage:Url"]));
            //services.AddScoped<IQueueStorage>(s => new QueueStorage(Configuration["ConnectionStrings:AzureStorage"]));

            AuthorizationHelper.ConfigureService(services);

            services.AddControllers();

            services.AddMvc().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //
            app.UseHttpRequestBodyMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseErrorHandlingMiddleware();

            if (Configuration["EnableCheckAllowedIPs"] != null && bool.Parse(Configuration["EnableCheckAllowedIPs"].ToString()))
            {
                // Use WhiteList (Read it from app settings or json file)
                //app.UseWhiteListMiddleware(Configuration["AllowedIPs"]);
                app.UseWhiteListMiddleware();
            }

            app.UseRouting();

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"[{env.EnvironmentName}] Web API V1");
                c.RoutePrefix = "";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

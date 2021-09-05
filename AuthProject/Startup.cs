using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthProject.Business;
using AuthProject.Business.Interfaces;
using AuthProject.Models.Configuration;
using AuthProject.Models.Context;
using AuthProject.Repository;
using AuthProject.Repository.Interfaces;
using AuthProject.Services;
using AuthProject.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AuthProject
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
            // Pegando TokenConfigurations do appsettings e atribuindo a um objeto do tipo TokenConfiguration
            TokenConfiguration tokenConfiguration = new TokenConfiguration();

            new ConfigureFromConfigurationOptions<TokenConfiguration>(Configuration.GetSection("TokenConfigurations"))
                .Configure(tokenConfiguration);
                
            services.AddSingleton(tokenConfiguration);

            //////////////////////////////////
            // Authentication Configuration //
            //////////////////////////////////
            services.AddAuthentication(option => {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option => {
                option.TokenValidationParameters = new TokenValidationParameters{
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenConfiguration.Issuer,
                    ValidAudience = tokenConfiguration.Audience,
                    IssuerSigningKey = new
                                    SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfiguration.Secret))
                };
            });

            /////////////////////////////////
            // Authorization Configuration //
            /////////////////////////////////
            /*
            services.AddAuthorization(auth => {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                            .RequireAuthenticatedUser().Build());
            });*/

            services.AddControllers();

            // Configure Context DB
            string connectionString = Configuration["ConnectionStrings:MysqlConnectionString"];
            services.AddDbContext<ApplicationContext>(options => 
                            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Inject Dependency (DI)
            services.AddTransient<ITokenService, TokenService>(); 
            services.AddTransient<IUserRepository, UserRepository>(); 
            services.AddScoped<ILoginBusiness, LoginBusiness>();


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthProject", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthProject v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Usando Autenticação e Autorização
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

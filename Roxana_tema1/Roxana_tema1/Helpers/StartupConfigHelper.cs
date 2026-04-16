using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using DataPersistance;
using Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Roxana_tema1.Middleware.Auth;
using Services;

namespace Roxana_tema1.Helpers
{
    public static class StartupConfigHelper
    {
        public static void ConfigureAppServices(this IServiceCollection services, IConfiguration configuration)
        {
          
            // add controllers
            services.AddControllers();
            ConfigureAuthServices(services, configuration);

            services.AddProblemDetails();

            ConfigureSwagger(services);
        }
        private static void ConfigureSwagger(IServiceCollection services)
        {
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Books API",
                    Description = "A simple example ASP.NET Core Web API"
                });
                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "Endangered Animals API V2",
                    Description = "A simple example ASP.NET Core Web API"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter only the JWT token"
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                // Set the comments path for the Swagger JSON and UI
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
               // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
              //  c.IncludeXmlComments(xmlPath);
            });
        }

        private static void ConfigureAuthServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["AuthorizationSettings:Issuer"],
                        ValidAudience = configuration["AuthorizationSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["AuthorizationSettings:Secret"] ?? string.Empty)
                        ),
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var email = context.Principal
                                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                            var userService = context.HttpContext.RequestServices
                                .GetRequiredService<IUserService>();

                            var user = userService.GetUserByEmail(email);

                            if (user == null)
                            {
                                context.Fail("User not found");
                                return;
                            }

                            context.HttpContext.Items["User"] = user;
                        }
                    };

                });

            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.User, policy =>
                    policy.Requirements.Add(new PrivilegeRequirement(Policies.User)));

                config.AddPolicy(Policies.Admin, policy =>
                    policy.Requirements.Add(new PrivilegeRequirement(Policies.Admin)));

                config.AddPolicy(Policies.All, policy =>
                    policy.Requirements.Add(new PrivilegeRequirement(Policies.All)));
            });

            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        private static void UseAuth(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }   




        private static void UseSwagger(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Books");

                // Serve Swagger UI at the root URL
                c.RoutePrefix = string.Empty;
            });
        }
    }
}

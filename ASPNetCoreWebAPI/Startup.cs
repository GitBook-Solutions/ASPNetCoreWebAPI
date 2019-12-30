using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPNetCoreWebAPI.DataServices;
using ASPNetCoreWebAPI.Helpers;
using ASPNetCoreWebAPI.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace ASPNetCoreWebAPI {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddDbContext<DataContext> (x => x.UseSqlServer (Configuration["ConnectionString:WebApiDB"]));
            services.AddAutoMapper (AppDomain.CurrentDomain.GetAssemblies ());
            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_2);
            services.AddScoped<IUserService, UserService> ();
            services.AddSwaggerGen (c => {
                c.SwaggerDoc ("v2", new Info { Title = "My API", Version = "v2" });
                c.AddSecurityDefinition ("Bearer", new ApiKeyScheme {
                    In = "header",
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Authorization",
                        Type = "apiKey"
                });

                c.AddSecurityRequirement (new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } }
                });

            });
            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer (options => {
                    var serverSecret = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (Configuration["JWT:key"]));
                    options.TokenValidationParameters = new
                    TokenValidationParameters {
                        IssuerSigningKey = serverSecret,
                        ValidIssuer = Configuration["JWT:Issuer"],
                        ValidAudience = Configuration["JWT:Audience"]
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts ();
            }
            app.UseAuthentication ();
            app.UseHttpsRedirection ();
            app.UseMvc ();
            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ("/swagger/v2/swagger.json", "My API V2");
            });

        }
    }
}
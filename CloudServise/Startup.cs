using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CloudService_API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CloudServise
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            var filePathSettings = Configuration.GetSection("AppSettings:FilePathSettings").Get<FilePathSettings>() ?? new FilePathSettings();
            services.AddTransient(t => new FilePathSettings());
            services.AddScoped(s => new FilePathSettings(filePathSettings));

            var passwordHashSettings = Configuration.GetSection("AppSettings:PasswordHashSettings").Get<PasswordHashSettings>() ?? new PasswordHashSettings();
            services.AddTransient(t => new PasswordHashSettings());
            services.AddScoped(s => new PasswordHashSettings(passwordHashSettings));

            var mailSettings = Configuration.GetSection("AppSettings:MailSettings").Get<MailSettings>() ?? new MailSettings();
            services.AddTransient(t => new MailSettings());
            services.AddScoped(s => new MailSettings(mailSettings));

            ValidateAppSettings(filePathSettings, passwordHashSettings, mailSettings);


            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MemoryBufferThreshold = int.MaxValue;
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Использование SSL
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,

                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,

                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });

            services.AddCors(o => o.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader()));

            services.AddControllers();

            services.AddSwaggerGen(options => { 
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Place to add JWT with Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer" },
                            Name = "Bearer"},
                            new List<string>()
                    }
                });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("AllowAll");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudService API");
            });
        }



        private static void ValidateAppSettings(FilePathSettings filePathSettings, PasswordHashSettings passwordHashSettings, MailSettings mailSettings)
        {
            var resultsValidation = new List<ValidationResult>();

            Validator.TryValidateObject(filePathSettings, new ValidationContext(filePathSettings), resultsValidation, true);
            Validator.TryValidateObject(passwordHashSettings, new ValidationContext(passwordHashSettings), resultsValidation, true);
            Validator.TryValidateObject(mailSettings, new ValidationContext(mailSettings), resultsValidation, true);

            resultsValidation.ForEach(error => Console.WriteLine(error.ErrorMessage));
            
        }
    }
}

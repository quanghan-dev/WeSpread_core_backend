using BLL.Constant;
using BLL.Dto;
using BLL.Filter;
using BLL.Service;
using BLL.Service.Impl;
using BLL.SignalRHub;
using DAL.Model;
using DAL.Repository;
using DAL.UnifOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Text;
using Twilio.Clients;

namespace API
{
    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            //Add DB connection
            services.AddDbContext<WeSpreadCoreContext>(opt => opt.UseSqlServer(
                _configuration.GetConnectionString("WeSpreadConnection")));

            services.AddControllers();

            //add CORS
            services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolicy",
                                  builder =>
                                  {
                                      builder.WithOrigins(_configuration.GetValue<string>("ServerLink"))
                                                   .AllowAnyHeader()
                                                  .AllowAnyMethod(); ;
                                  });
            });

            //Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WeSpread",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            //Add SignalR
            services.AddSignalR();

            //Add Twilio connection
            services.AddHttpClient<ITwilioRestClient, CustomTwilioClient>();

            //Add JWT Authentication
            var key = _configuration.GetValue<string>("SecretKey");

            //Jwt Authentication
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                x.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Call this to skip the default logic and avoid using the default response
                        context.HandleResponse();

                        // Write to the response
                        context.Response.StatusCode = 401;
                        BaseResponse<string> response = new BaseResponse<string>
                        {
                            ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                            ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                        };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                    }
                };
            });

            services.AddSingleton<IJwtAuthenticationManager>(
                new JwtAuthenticationManager(key));

            //Add Redis
            services.AddStackExchangeRedisCache(opt =>
            {
                opt.Configuration = _configuration.GetValue<string>
                ("CacheSettings:ConnectionString");
            });

            //Add Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Add Mapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //Add services
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IPersistentLoginService, PersistentLoginService>();
            services.AddScoped<IUtilService, UtilService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IUploadFirebaseService, UploadFirebaseService>();
            services.AddScoped<IValidateDataService, ValidateDataService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IDonationSessionService, DonationSessionService>();
            services.AddScoped<IMoMoService, MoMoService>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<IRecruitmentSessionService, RecruitmentSessionService>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IDonateService, DonateService>();
            services.AddScoped<IRegistrationFormService, RegistrationFormService>();
            services.AddScoped<ILocationService, LocationService>();

            //Add Logger
            services.AddSingleton<ILogger, LogNLog>();

            services.AddMvc(options =>
            {
                options.Filters.Add(new ErrorHandlingFilter());
            });
        }

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

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeSpread v1");
            });

            app.UseCors("MyPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SignalRHubService>("/hub");
            });
        }
    }
}

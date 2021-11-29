using AutoMapper;
using ExigoResourceSet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Text;
using WinkNatural.Web.Common.Extensions;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Services;
using WinkNaturals.AuthantictionMiddleware;
using WinkNaturals.Helpers;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Models.Shopping.PointAccount;
using WinkNaturals.Models.Shopping.PointAccount.Interfaces;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using static WinkNaturals.Models.UnitedStatesConfiguration;

namespace WinkNatural.Web.WinkNaturals
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.IActionContextAccessor, Microsoft.AspNetCore.Mvc.Infrastructure.ActionContextAccessor>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IShoppingService, ShoppingService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPartyService, PartyService>();
            //services.AddScoped<IServiceItem, ServiceItem>();
            services.AddScoped<ISqlCacheService, DistributedCache>();
            services.Configure<ConfigSettings>(option => Configuration.GetSection("Settings").Bind(option));
            services.Configure<ConnectionStrings>(option => Configuration.GetSection("ConnectionStrings").Bind(option));
            services.AddSingleton<IExigoApiContext, ExigoApiContext>();
            services.AddSingleton<ICacheProvider, SqlInMemoryCacheProvider>();
            services.AddScoped<IGetCurrentMarket, GetCurrentMarket>();
            services.AddScoped<ICache, CacheService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPropertyBags, PropertyBags>();
            services.AddScoped<IPropertyBagItem, PropertyBagItemDetail>();
            services.AddScoped<IOrderConfiguration, OrderItemConfiguration>();
            services.AddScoped<IMarketConfigurationSetting, MarketConfigurationSetting>();
            services.AddScoped<ICustomerPointAccount, PointAccountRepo>();
            services.AddScoped<IAutoOrders, AutoOrders>();
            services.AddScoped<ICustomerAutoOreder, CustomerAutoOreder>();
            services.AddScoped<IOrderConfiguration, BaseOrderConfiguration>();

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString =
                Configuration.GetSection("ConnectionStrings:DefaultConnectionforCache").Value;
                options.SchemaName = "dbo";
                options.TableName = "TestCache";
            });

            services.AddDistributedSqlServerCache(options =>
            {
                options.DefaultSlidingExpiration = TimeSpan.FromMinutes(10);
            });


            services.AddDistributedSqlServerCache(options =>
            {
                options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(6);
            });


            //var CS = Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
            // services.Configure<CacheConfig>(Configuration.GetSection("ConnectionStrings:DefaultConnection"));
            services.AddControllers();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            //        services.AddControllers().AddJsonOptions(x =>
            //x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.ig);

            services.AddControllers().AddNewtonsoftJson(x =>
            x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            IMapper mapper = mappingConfig.CreateMapper();

            //ExigoConfig exigoconfig = Configuration
            //.GetSection(nameof(ExigoConfig))
            //.Get<ExigoConfig>();
            //ExigoConfig.Instance = exigoconfig;

            services.AddSingleton(mapper);
            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {

                        Title = "WinkNaturals",
                        Version = "v1"
                    });
                setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });
            //Exigo resourceset  configuration
            ResourceSetManager.Start(new ResourceSetUpdaterOptions
            {
                SubscriptionKeys = Configuration.GetSection("Settings:ExigoResourceSetConfig:SubscriptionKeys").Value,
                EnvironmentCode = Configuration.GetSection("Settings:ExigoResourceSetConfig:EnvironmentCode").Value,

                LoginName = Configuration.GetSection("Settings:ExigoConfig:LoginName").Value,
                Password = Configuration.GetSection("Settings:ExigoConfig:Password").Value,
                Company = Configuration.GetSection("Settings:ExigoConfig:CompanyKey").Value,

                LocalPath = Path.Combine(Environment.ContentRootPath, "App_Data")

            });

            var secret = Configuration.GetSection("Settings:JwtSettings:Key").Value;
            var key = Encoding.ASCII.GetBytes(secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });


            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WinkNaturals", Version = "v1" });
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WinkNaturals v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Environment.WebRootPath, "assets/images")),
                RequestPath = "/files"
            });
            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMiddleware<AuthMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}


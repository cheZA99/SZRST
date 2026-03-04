using Application.Interfaces;
using Application.Mapper;
using Application.Services;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using SZRST.API.Controllers;
using SZRST.Application.Services.MailService;
using SZRST.Shared.Middleware;
using WebApi.Error;

namespace SZRST.WebApi
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
			services.AddCors(option =>
			  option.AddPolicy("MyPolicy", builder =>
			  builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

			services.AddHttpContextAccessor();
			services.AddScoped<ICurrentUserService, CurrentUserService>();
			services.AddScoped<ITenantProvider, TenantProvider>();

			services.AddIdentity<User, Role>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequiredLength = 5;
			}).AddEntityFrameworkStores<SZRSTContext>()
			.AddDefaultTokenProviders();
			services.AddScoped<IAuthService, AuthService>();
			services.AddDbContext<SZRSTContext>((sp, options) =>
			{
				var tenantProvider = sp.GetRequiredService<ITenantProvider>();
				options.UseSqlServer(Configuration.GetConnectionString("SZRST"));
			});
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.Events = new JwtBearerEvents
				{
					OnChallenge = context =>
				  {
					  context.HandleResponse();
					  context.Response.StatusCode = 401;
					  return Task.CompletedTask;
				  }
				};

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,

					ValidIssuer = Configuration["AuthSettings:Issuer"],
					ValidAudience = Configuration["AuthSettings:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(Configuration["AuthSettings:Key"])
				 ),
					ClockSkew = TimeSpan.Zero
				};
			});

			services.AddControllers();
			services.AddAutoMapper(typeof(MapperProfile));
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "SZRST.API", Version = "v1" });

				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
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
		  new string[] {}
	   }
	  });
			});
			services.AddPersistence(Configuration);
			services.AddAuthorization();

			#region Binding

			services.AddSingleton<ProblemDetailsFactory, UserManagmentProblemDetailsFactory>();
			services.AddTransient<IMailService, SendGridMailService>();

			services.AddTransient<FacilityController>();
			services.AddTransient<LocationController>();

			#endregion Binding
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			using (var scope = app.ApplicationServices.CreateScope())
			{
				IdentitySeed.SeedAsync(scope.ServiceProvider).Wait();
			}
			app.UseSwagger();
			app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SZRST.API v1"));
			app.UseHttpsRedirection();
			app.UseMiddleware<ExceptionMiddleware>();
			app.UseCors("MyPolicy");
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			Console.WriteLine("Loaded CS = " + Configuration.GetConnectionString("SZRST"));

			Console.WriteLine("ENV = " + env.EnvironmentName);
			Console.WriteLine("CS = " + Configuration.GetConnectionString("SZRST"));
		}
	}
}
using Application.Interfaces;
using Application.Mapper;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
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
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SZRST.API.Controllers;
using SZRST.API.Security;
using SZRST.Application.Services.MailService;
using SZRST.Domain.Constants;
using SZRST.Shared.Middleware;
using SZRST.Web.Schedule;
using SZRST.Web.Serivces;
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
					OnTokenValidated = async context =>
					{
						var principal = context.Principal;
						if (principal?.Identity?.IsAuthenticated != true)
						{
							context.Fail("Neispravan token.");
							return;
						}

						if (principal.IsInRole(Roles.SuperAdmin))
						{
							return;
						}

						if (principal.IsInRole(Roles.Korisnik))
						{
							return;
						}

						var tenantClaim = principal.FindFirst("tenantId")?.Value;
						if (!int.TryParse(tenantClaim, out var tenantId) || tenantId <= 0)
						{
							context.Fail("Tenant claim nedostaje ili nije validan.");
							return;
						}

						var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
						var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
						var user = string.IsNullOrWhiteSpace(userId) ? null : await userManager.FindByIdAsync(userId);

						if (user == null || user.TenantId != tenantId)
						{
							context.Fail("Tenant claim nije usklađen sa korisnikom.");
						}
					},
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

			services.AddHangfire(config =>
			{
				config.UseSqlServerStorage(
				 Configuration.GetConnectionString("SZRST"),
				 new Hangfire.SqlServer.SqlServerStorageOptions
				 {
					 PrepareSchemaIfNecessary = true,
					 QueuePollInterval = TimeSpan.Zero,
					 UseRecommendedIsolationLevel = true,
					 DisableGlobalLocks = true
				 });
			});

			services.AddHangfireServer();

			services.AddControllers();
			services.AddFluentValidationAutoValidation();
			services.AddValidatorsFromAssemblyContaining<RegisterViewModelValidator>();
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

			services.AddScoped<IReservationReportService, ReservationReportService>();
			services.AddScoped<ReportService>();

			#endregion Binding
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			QuestPDF.Settings.License = LicenseType.Community;

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
			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = new[] { new AdminDashboardAuthorizationFilter() }
			});
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

			RecurringJob.AddOrUpdate<ReportService>(
			    "monthly-reports",
			    service => service.GenerateMonthlyReports(),
			    "0 0 1 * *"
			);

			Console.WriteLine("ENV = " + env.EnvironmentName);
		}
	}
}

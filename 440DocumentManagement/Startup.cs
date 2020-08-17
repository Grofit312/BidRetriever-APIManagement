using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.SimpleEmail;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.Lambda;
using _440DocumentManagement.Services.Interface;
using _440DocumentManagement.Services.Concrete;
using System;
using Npgsql;
using System.Data;

namespace _440DocumentManagement
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
			// Register the Swagger services
			services.AddSwaggerDocument(config =>
			{
				config.PostProcess = document =>
				{
					document.Info.Version = "v1";
					document.Info.Title = "Document Management API";
				};
			});

			services.AddMvc();
			services.AddCors();

			// Configure AWS
			services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
			services.AddAWSService<IAmazonSimpleEmailService>();
			services.AddAWSService<IAmazonS3>();
			services.AddAWSService<IAmazonDynamoDB>();
			services.AddAWSService<IAmazonLambda>();

			// Add Services
			AddServices(services);

			// Configure Database Connection
			var br_db_host_name = System.Environment.GetEnvironmentVariable("BR_DB_HOST_NAME");
			if (string.IsNullOrWhiteSpace(br_db_host_name))
			{
				throw new Exception("Missing environment variable: 'BR_DB_HOST_NAME'");
			}

			var br_db_port = System.Environment.GetEnvironmentVariable("BR_DB_PORT");
			if (string.IsNullOrWhiteSpace(br_db_port))
			{
				throw new Exception("Missing environment variable: 'BR_DB_PORT'");
			}

			var br_db_user_name = System.Environment.GetEnvironmentVariable("BR_DB_USER_NAME");
			if (string.IsNullOrWhiteSpace(br_db_user_name))
			{
				throw new Exception("Missing environment variable: 'BR_DB_USER_NAME'");
			}

			var br_db_password = System.Environment.GetEnvironmentVariable("BR_DB_PASSWORD");
			if (string.IsNullOrWhiteSpace(br_db_password))
			{
				throw new Exception("Missing environment variable: 'BR_DB_PASSWORD'");
			}

			var br_db_name = System.Environment.GetEnvironmentVariable("BR_DB_NAME");
			if (string.IsNullOrWhiteSpace(br_db_name))
			{
				throw new Exception("Missing environment variable: 'BR_DB_NAME'");
			}

			var connString = String.Format("Server={0};Port={1};Username={2};Password={3};Database={4};" +
				"Pooling=true;MinPoolSize=0;MaxPoolSize=50;Timeout=60;ConnectionIdleLifetime=5;ConnectionPruningInterval=1;",
				br_db_host_name, br_db_port, br_db_user_name, br_db_password, br_db_name);
			services.AddTransient<IDbConnection>((sp) => new NpgsqlConnection(connString));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			// Register the Swagger generator and the Swagger UI middlewares
			app.UseOpenApi();
			app.UseSwaggerUi3();

			app.UseCors(builder => builder
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()
				.AllowCredentials());

			app.UseMvc();
		}

		private void AddServices(IServiceCollection services)
		{
			services.AddTransient<IBaseService, BaseService>();

			services.AddTransient<ICustomerAttributeManagementService, CustomerAttributeManagementService>();
			services.AddTransient<IDashboardManagementService, DashboardManagementService>();
			services.AddTransient<IDashboardPanelManagementService, DashboardPanelManagementService>();
			services.AddTransient<IDataSourceManagementService, DataSourceManagementService>();
			services.AddTransient<IDataSourceFieldManagementService, DataSourceFieldManagementService>();
			services.AddTransient<IDataViewManagementService, DataViewManagementService>();
			services.AddTransient<IDataViewFieldSettingManagementService, DataViewFieldSettingManagementService>();
			services.AddTransient<IDataViewFilterManagementService, DataViewFilterManagementService>();
			services.AddTransient<IDocumentManagementService, DocumentManagementService>();
			services.AddTransient<IProjectRelationshipService, ProjectRelationshipService>();
			services.AddTransient<ISystemAttributeManagementService, SystemAttributeManagementService>();
		}
	}
}

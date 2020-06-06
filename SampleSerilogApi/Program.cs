using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace SampleSerilogApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(Configuration)
				.Enrich.FromLogContext()
			.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
			{
				AutoRegisterTemplate = true,
			})
			.CreateLogger();

			try
			{
				Log.Information("Starting the web host");

				CreateHostBuilder(args).Build().Run();

				Log.Information("Shutting down the web host");
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>()
						.UseSerilog();
				});
	}
}

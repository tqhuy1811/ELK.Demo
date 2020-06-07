using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;

namespace ELK.Demo
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
			services.AddControllers();

			var node = new Uri("http://localhost:9200");
			Data.Seed();

			var settings = new ConnectionSettings(node)
				.EnableDebugMode() // use in conjunction with other debug settings
				.PrettyJson()
				.DisableDirectStreaming(); // https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/debug-information.html
			services.AddSingleton(new ElasticClient(settings));
		}

		public void Configure(IApplicationBuilder app,
			IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseDefaultFiles();
			app.UseStaticFiles();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}
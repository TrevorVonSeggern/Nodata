using Autofac.Extensions.DependencyInjection;

namespace SampleEFCoreApi;

public static class Program
{
	public static void Main(string[] args)
	{
		BuildWebHost(args).Build().Run();
	}

	public static IHostBuilder BuildWebHost(string[] args)
	{
		return Host.CreateDefaultBuilder(args)
			.UseServiceProviderFactory(new AutofacServiceProviderFactory())
			.ConfigureWebHostDefaults(webBuilder => {
				webBuilder.UseUrls("http://localhost:3000");
				webBuilder.UseStartup<Startup>();
			})
			;
	}
}

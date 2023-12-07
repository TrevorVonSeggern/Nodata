using Microsoft.EntityFrameworkCore;
using Autofac;
using NoData;

namespace SampleEFCoreApi;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddOptions();
		services.AddControllers();
		services.AddNoData();
	}

	public void ConfigureContainer(ContainerBuilder builder)
	{
		builder.Register(x => new DbContextOptionsBuilder<DataContext>()
			// .UseSqlServer(@"Server=localhost;Database=MyDb;User=sa;Password=YourStrong!Passw0rd;")
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			// .UseLoggerFactory(MyLoggerFactory)
			)
			.As<DbContextOptionsBuilder<DataContext>>().As<DbContextOptionsBuilder>();

		builder.Register<DataContext>(x => new DataContext(x.Resolve<DbContextOptionsBuilder<DataContext>>().Options)).SingleInstance();

		AutoMapperConfig.DependencyInjection(builder);
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		app.UseRouting();
		app.UseEndpoints(x => x.MapControllers());
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Console;
using NoData;

namespace SampleEFCoreApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }

        public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddControllersAsServices();

            services.AddNoData();

            var builder = new ContainerBuilder();

            builder.Populate(services);
            ConfigureContainer(builder);

            ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(ApplicationContainer);
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}

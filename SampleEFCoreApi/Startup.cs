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

namespace SampleEFCoreApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // var databaseOptions = new DbContextOptionsBuilder<DataContext>().UseInMemoryDatabase(databaseName: "MyCustomDatabase").Options;
            // var context = new DataContext(databaseOptions);
            // builder.Register<DataContext>(x => context).AsSelf().SingleInstance();

            AutoMapperConfig.DependencyInjection(builder);
        }

        public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddControllersAsServices();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(NoData.NoDataQueryBuilder<>));

            var connection = @"Server=localhost;Database=MyDb;User=sa;Password=YourStrong!Passw0rd;";
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connection).UseLoggerFactory(MyLoggerFactory));
            // var db = new DataContext(new DbContextOptions()(connection));

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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NoData
{
    public static class NoDataDI
    {
        public static void AddNoData(this IServiceCollection services)
        {
            services.AddScoped(typeof(NoData.SettingsForType<>));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(NoData.NoDataBuilder<>));
            services.AddScoped(typeof(NoData.INoData<>), typeof(NoData.NoDataBuilder<>));

            services.AddScoped(typeof(NoData.Parameters),
                provider => ParametersHelper.FromHttpContext(provider.GetRequiredService<IHttpContextAccessor>()));
        }
    }
}

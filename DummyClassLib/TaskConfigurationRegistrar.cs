using Abstractions;
using DummyClassLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace WebApplication4
{
    public class TaskConfigurationRegistrar : IConfigRegistrar
    {
        public string[] Configure()
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return new[]
            {
                Path.Combine(assemblyLocation, "appsettings.json"),
                Path.Combine(assemblyLocation, "appsettings1.json")
            };

        }

        public void Register(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddTransient<IJob, Job>();
            builder.Services.AddTransient<IFakeService, FakeService>();
            builder.Services.Configure<SubConfig>(builder.Configuration.GetSection("SubConfig"));

        }
    }
}

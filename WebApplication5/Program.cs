using Abstractions;
using Hangfire;
using System.Reflection;
using Hangfire.Dashboard;
using Microsoft.Extensions.Primitives;
using WebApplication5;
using static ChangeTokenSample.Utilities.Utilities;
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

 byte[] _appsettingsHash = new byte[20];
 byte[] _appsettingsEnvHash = new byte[20];
foreach (var dllFile in Directory.GetFiles(
             Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\assembly", "*.dll"))
{
    var dll = Assembly.LoadFrom(dllFile);
    var iregistrars = dll.GetTypes()
        .Where(mytype => mytype.GetInterfaces().Contains(typeof(IConfigRegistrar))).ToList();

    var sources = new List<string>();
    foreach (var item in from classType in iregistrars
             let classInst = Activator.CreateInstance(classType)
             let methodInfo = classType.GetMethod("Configure")
             where methodInfo != null
             select (string[]) methodInfo.Invoke(classInst, null)
             into result
             where result != null
             from item in result
             select item)
    {
        sources.Add(item);
        sources.Add(ConfigManipulator.GetUpdatableFilePath(item));
    }

    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();

        var env = hostingContext.HostingEnvironment;
        config.AddJsonFile("appsettings.json", true, true);
        config.AddJsonFile($"appsettings{ConfigManipulator.JsonAlternateName}", true, true);

        foreach (var source in sources)
        {
            if (!File.Exists(source) && source != null) File.WriteAllText(source, "{}");
            config.AddJsonFile(source, true, true);
        }

        config.AddEnvironmentVariables();

        //if (args != null)
        //{
        //    config.AddCommandLine(args);
        //}
    });
    services.AddSingleton<IConfigurationRoot>(builder.Configuration);
    foreach (var classType in iregistrars)
    {
        if (classType != null)
        {
            // Create class instance.
            var classInst = Activator.CreateInstance(classType);

            // Invoke required method.
            var methodInfo = classType.GetMethod("Register");
            if (methodInfo != null)
            {
                methodInfo.Invoke(classInst, new object[] {builder});
            }
        }
    }
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<Test>(builder.Configuration.GetSection("Test"));

services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());


// Add the processing server as IHostedService
services.AddHangfireServer();

var app = builder.Build(); 
 void InvokeChanged(IWebHostEnvironment webHostEnvironment)
        {
    byte[] appsettingsHash = ComputeHash("appsettings.json");
 

    if (!_appsettingsHash.SequenceEqual(appsettingsHash) )
    {
        _appsettingsHash = appsettingsHash;
       

        WriteConsole("Configuration changed (Simple Startup Change Token)");
    }
}
ChangeToken.OnChange(
    () => app.Configuration.GetReloadToken(),
    InvokeChanged,
    app.Environment);
app.UseHangfireDashboard();
var job = app.Services.GetService<IJob>();
var aaa = app.Services.GetService<IFakeService>();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHangfireDashboard();
app.Run();
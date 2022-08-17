using Abstractions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
foreach (string dllFile in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\assembly","*.dll"))
{

   var DLL= Assembly.LoadFrom(dllFile);
    var iregistrars = DLL.GetTypes()
                .Where(mytype => mytype.GetInterfaces().Contains(typeof(IConfigRegistrar)));

    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();

        var env = hostingContext.HostingEnvironment;

        foreach (var classType in iregistrars)
        {
            if (classType != null)
            {
                // Create class instance.
                var classInst = Activator.CreateInstance(classType);

                // Invoke required method.
                MethodInfo methodInfo = classType.GetMethod("Register");
                if (methodInfo != null)
                {
                    object result = null;
                    result = methodInfo.Invoke(classInst, new object[] { builder });
                    result.ToString();
                }
            }
        }
      


        config.AddEnvironmentVariables();

        //if (args != null)
        //{
        //    config.AddCommandLine(args);
        //}
    });

    foreach (Type classType in iregistrars)
    {
        if (classType != null)
        {
            // Create class instance.
          var  classInst = Activator.CreateInstance(classType);

            // Invoke required method.
            MethodInfo methodInfo = classType.GetMethod("Register");
            if (methodInfo != null)
            {
                object result = null;
               result = methodInfo.Invoke(classInst, new object[] { builder });
                result.ToString();
            }
        }
    }

 

}

    var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

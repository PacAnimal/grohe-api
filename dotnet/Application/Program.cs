using Application;
using Cathedral.API.BasicAuth;
using Cathedral.API.Swagger;
using Cathedral.Config;
using Cathedral.Extensions;
using Cathedral.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

// configuration
var config = Env.Config;

// build test?
var buildtest = args.Contains("buildtest");

// add a basic web api
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// swagger stuff
services.AddMvcCore().AddSaneJsonOptions();
services.AddEndpointsApiExplorer();
services.AddControllers();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GroheAPI", Version = "v1" });
    
    // custom type names for swagger
    c.UseJsonTypeNameAttribute();
    
    // make swagger aware of basic auth
    c.UseBasicAuth();
});

// services
services.AddEnvironmentConfiguration();
services.AddMemoryCache();
services.AddSingleton<IApiClient, ApiClient>();
services.AddSingleton<OrderedSemaphore<IApiClient>>();
services.AddHostedService<NotificationPollerService>();

// set loglevels to make things a bit quieter
services.AddSereneConsoleLogging();

// lower case routes please
services.UseLowerCaseRoutes();

// data protection, or lack thereof
services.AddDataProtection().PersistKeysToNowhere();

// add basic auth
services.AddBasicAuth();

// set port number
builder.SetKestrelPort(int.Parse(config.GetString("LOCAL_PORT", "5000")));

// build it
var app = builder.Build();

// swagger again
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GroheAPI v1"));

// add redirect from root to swagger
app.RedirectToSwagger();

// map controllers, with authorization (this is what makes them work, not just be visible in swagger)
app.MapControllersWithBasicAuth();

// create a scope and initialize
using (var scope = app.Services.CreateScope())
{
    var log = scope.GetLogger<Program>();
    var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>() as ApiClient ?? throw new Exception("ApiClient not found or unexpected type");
    
    // Hello World!
    log.LogInformation("Program Running...");

    // exit if this is a build test
    if (buildtest)
    {
        log.LogInformation("Build test complete");
        return;
    }
    
    // do the initial login, verifying our credentials work
    await apiClient.Login();
}

// run it!
app.Run();

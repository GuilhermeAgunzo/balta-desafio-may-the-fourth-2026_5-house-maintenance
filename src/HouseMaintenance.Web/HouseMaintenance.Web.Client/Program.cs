using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HouseMaintenance.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IMaintenancePlannerClient, MaintenancePlannerClient>();

await builder.Build().RunAsync();

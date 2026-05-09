using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Neon.Mapbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var mapboxToken = builder.Configuration["Mapbox:AccessToken"]
    ?? System.Environment.GetEnvironmentVariable("MAPBOX_ACCESS_TOKEN")
    ?? string.Empty;

builder.Services.AddMapbox(mapboxToken);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<TestMapbox.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

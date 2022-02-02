using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using PdfGenerator;

Console.WriteLine("Installing browsers");

// The following line installs the default browsers. If you only need a subset of browsers,
// you can specify the list of browsers you want to install among: chromium, chrome,
// chrome-beta, msedge, msedge-beta, msedge-dev, firefox, and webkit.

Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", PlatformServices.Default.Application.ApplicationBasePath);
var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
// var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
if (exitCode != 0)
{
    Console.WriteLine("Failed to install browsers");
    Environment.Exit(exitCode);
}

Console.WriteLine("Browsers installed");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMvc().AddRazorRuntimeCompilation();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMemoryCache()
    .AddTransient<IReportProvider, MockReportProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection()
    .UseAuthorization();
app.MapControllers();

app.Run();
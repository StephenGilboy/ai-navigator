using System.Reflection;
using AiNavigator.BrowserAgent;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings());
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

var key = builder.Configuration.GetValue("OPEN_AI_KEY", "");
if (string.IsNullOrEmpty(key))
    throw new NullReferenceException("OPEN_AI_KEY environment variable is not set");

builder.Services.AddSingleton<BrowserApi>();
builder.Services.AddSingleton(new OpenAIClient(key));
builder.Services.AddSingleton<BrowserAgent>();
builder.Services.AddHostedService<WorkerService>();

var app = builder.Build();

await app.StartAsync();

// var browserApi = new BrowserApi();
// var content = await browserApi.Navigate("https://www.google.com");
// Console.Write(content);
// Thread.Sleep(2000);

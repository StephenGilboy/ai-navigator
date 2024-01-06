using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Playwright;

namespace AiNavigator.BrowserAgent;

public record NavigateArgs(string url, bool openNewPage = false);

public class BrowserApi
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private IPage _page;
    
    public BrowserApi()
    {
        _playwright = Playwright.CreateAsync().Result;
        _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
        {
            Headless = false
        }).Result;
        _page = _browser.NewPageAsync().Result;
    }
    
    public async Task<string> Navigate(NavigateArgs args)
    {
        if (args.openNewPage)
        {
            await _page.CloseAsync();
            _page = await _browser.NewPageAsync();
        }
        await _page.GotoAsync(args.url);
        return await _page.ContentAsync();
    }
}

public static class BrowserApiChatFunctions
{
    public static readonly ChatCompletionsFunctionToolDefinition Navigate = new()
    {
        Name = "navigate",
        Description = "Navigates to a URL and returns the page contents",
        Parameters = BinaryData.FromObjectAsJson(new
        {
            Type = "object",
            Properties = new
            {
                Url = new
                {
                    Type = "string",
                    Description = "The URL to navigate to"
                },
                OpenNewPage = new
                {
                    Type = "boolean",
                    Description = "Whether to open a new page or not"
                }
            },
            Required = new[] { "url" }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
    };
}
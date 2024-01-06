using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;

namespace AiNavigator.BrowserAgent;

public class BrowserAgent(OpenAIClient client, BrowserApi browserApi)
{
    public async Task<string> DoStuff(string message, CancellationToken cancellationToken)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = "gpt-3.5-turbo-1106",
            Messages =
            {
                new ChatRequestSystemMessage("You are an expert at using Microsoft's Playwright. You have access to functions that allow you to interact with the browser."),
                new ChatRequestUserMessage(message)
            },
            Tools = { BrowserApiChatFunctions.Navigate }
        };

        Response<ChatCompletions> response = await 
            client.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        var responseMessage = response.Value.Choices[0].Message;
        
        // All done
        if (responseMessage.ToolCalls.Count <= 0)
            return responseMessage.Content;
        
        // Handle Tool Calls
        var messages = await ProcessToolCalls(responseMessage, chatCompletionsOptions.Messages);
        var secondChatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = "gpt-3.5-turbo-1106",
            Tools = { BrowserApiChatFunctions.Navigate },
        };
        foreach (var secondMessage in messages)
        {
            secondChatCompletionsOptions.Messages.Add(secondMessage);
        }
        response = await client.GetChatCompletionsAsync(secondChatCompletionsOptions, cancellationToken);
        responseMessage = response.Value.Choices[0].Message;

        return responseMessage.Content;
    }
    
    private async Task<IList<ChatRequestMessage>> ProcessToolCalls(ChatResponseMessage chatMessage, IList<ChatRequestMessage> messages)
    {
        var newMessages = new List<ChatRequestMessage>().Concat(messages).ToList();
        newMessages.Add(new ChatRequestAssistantMessage(chatMessage));
        foreach (var toolCall in chatMessage.ToolCalls)
        {
            var fnToolCall = toolCall as ChatCompletionsFunctionToolCall;
            if (fnToolCall == null)
                continue;
            switch (fnToolCall.Name)
            {
                case "navigate":
                {
                    var fnArgs = JsonSerializer.Deserialize<NavigateArgs>(fnToolCall.Arguments);
                    if (fnArgs is null)
                        throw new ArgumentNullException(
                            $"Arguments from Tool call did not deserialize correctly {fnToolCall.Arguments}");
                    var content = await browserApi.Navigate(fnArgs);
                    newMessages.Add(new ChatRequestToolMessage(content, fnToolCall.Id));
                    break;
                }
                default:
                    continue;
            }
        }
        
        return newMessages;
    }
}
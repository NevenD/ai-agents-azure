using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_ENDPOINT' is not set");
var model = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";
//var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_API_KEY' is not set");

// Create an AI agent using the Azure OpenAI client with Azure CLI credentials
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(model)
    .AsAIAgent(instructions: "You are a helpful assistant. Keep answers brief, 2 sentences max");

// Create an AI agent using the Azure OpenAI client with an API key it is not recommended to use this method in production,
// use AzureCliCredential instead
//AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey))
//    .GetChatClient(model)
//    .AsAIAgent(instructions: "You are a helpful assistant. Keep answers brief.");

Console.WriteLine(await agent.RunAsync("What is the capital of Croatia?"));

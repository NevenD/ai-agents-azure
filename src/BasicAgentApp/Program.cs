using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_ENDPOINT' is not set");
var model = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

// Create an IChatClient using the Azure OpenAI client with Azure CLI credentials
IChatClient chatClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(model)
    .AsIChatClient();
// Define the Agent anatomy and instructions

AIAgent supportAgent = chatClient.AsAIAgent(
    name: "SupportAgent",
    instructions: "You are a Tier 1 IT support Agent. Your answers must be concise, professional and limited strictuly to network issues. " +
    "Any other queries should be politly declined. focus only on network issues related to the IT.");


Console.WriteLine($"Agent {supportAgent.Name} is online. \n");

string userIssue = "I am getting a DNS resolution error when connecting to the corporate VPN from a coffee shop.";
Console.WriteLine($"User Issue: {userIssue}");

AgentResponse response = await supportAgent.RunAsync(userIssue);
Console.WriteLine($"Agent: {response.Text}");
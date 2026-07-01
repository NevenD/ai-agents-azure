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

AIAgent agent = chatClient.AsAIAgent(
    name: "HistoryBuff",
    instructions: "You are a helful history teacher. you answer questions and help students make connections between historical events.");


// this object will accumulate the conversation history and context for the agent
AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("History Teacher is online. Type 'exit' to quit.\n");

// conversation loop

while (true)
{
    Console.Write("User:");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
    {
        break;
    }

    // We pass the 'session' object to the agent so it can maintain context across multiple turns of conversation
    // The framework automatically adds the user input to the sessioon
    // sends the full history to the clouzd, and appends the agent's response back to the session
    AgentResponse response = await agent.RunAsync(input, session);

    Console.WriteLine($"Agent: {response.Text}\n");
}

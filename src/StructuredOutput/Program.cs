using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text.Json.Serialization;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_ENDPOINT' is not set");
var model = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

// Create an IChatClient using the Azure OpenAI client with Azure CLI credentials
var agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(model)
    .AsAIAgent(
        name: "StructuredOutputAgent",
        instructions: "You are an AI analyst. Extract the topic, action items, and overall sentiment from the provided transcript."
    );

string transcript = "We discussed the Q4 marketing push. Sarah needs to finalize the budget by Tuesday. John will contact the ad agency. " +
    "Overall, everyone felt very optimistic about the campaign.";

Console.WriteLine($"Analyzing transcript: {transcript}\n");

// Run the agent to analyze the transcript and extract structured information
AgentResponse<MeetingAnalysis> response = await agent.RunAsync<MeetingAnalysis>(transcript);

MeetingAnalysis analysis = response.Result;

if (analysis is not null)
{
    Console.WriteLine($"Full analyzis: {analysis}\n");
    Console.WriteLine($"Topic: {analysis.Topic}\n");
    Console.WriteLine($"Sentiment: {analysis.Sentiment}\n");
    Console.WriteLine($"Action Items Count: {analysis.ActionItems.Count}\n");
    Console.WriteLine("Action Items:");
    foreach (var item in analysis.ActionItems)
    {
        Console.WriteLine($"- {item}");
    }

}


public record MeetingAnalysis
{
    [property: JsonPropertyName("topic")]
    public string Topic { get; init; } = string.Empty;

    [property: JsonPropertyName("actionItems")]
    public List<string> ActionItems { get; init; } = [];

    [property: JsonPropertyName("sentiment")]
    public string Sentiment { get; init; } = string.Empty;
}
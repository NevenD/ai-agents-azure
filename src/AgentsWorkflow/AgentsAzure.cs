using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

public class AgentsAzure
{
    private readonly IChatClient _chatClient;


    public AIAgent TriageAgent { get; }
    public AIAgent HardwareAgent { get; }
    public AIAgent SoftwareAgent { get; }

    public AgentsAzure(string endpoint, string deploymentName)
    {
        _chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
            .GetChatClient(deploymentName)
            .AsIChatClient();

        TriageAgent = _chatClient.AsAIAgent(
            name: "Triage",
            instructions: "Analyze the user's IT request. Categorize it strictly as either 'Hardware' or 'Software'. Output only the category word."
        );

        HardwareAgent = _chatClient.AsAIAgent(
            name: "HardwareSupport",
            instructions: "You are an enterprise hardware specialist. Provide concise troubleshooting steps for physical device issues."
        );

        SoftwareAgent = _chatClient.AsAIAgent(
             name: "SoftwareSupport",
             instructions: "You are an enterprise software specialist. Provide concise troubleshooting steps for application, OS, and network issues."
         );
    }
}
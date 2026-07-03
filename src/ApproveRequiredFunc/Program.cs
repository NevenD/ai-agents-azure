
// define the sensitive Enterprise Tool

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ComponentModel;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

internal static class FinanceTools
{
    [Description("Issues a financial refund to a customer. Use this ONLY when the user explicitly requests a refund and provides an Order ID.")]
    public static string IssueRefund(
        [Description("The Order ID to refund (e.g., ORD-12345).")] string orderId,
        [Description("The decimal amount to refund.")] decimal amount)
    {
        // Simulating a deterministic call to a payment gateway (e.g., Stripe or PayPal)
        Console.WriteLine($"\n[SYSTEM LOG] Executing secure transaction: Refunded ${amount} to {orderId}.\n");
        return $"SUCCESS: ${amount} has been refunded to order {orderId}.";
    }
}



internal class Program
{
    private static async Task Main(string[] args)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_ENDPOINT' is not set");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";


        AIFunction rawRefundFunction = AIFunctionFactory.Create(FinanceTools.IssueRefund);
        AIFunction secondRefundTool = new ApprovalRequiredAIFunction(rawRefundFunction);

        var agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
             .GetChatClient(deploymentName)
             .AsAIAgent(
                 name: "FinanceSupport",
                 instructions: "You are a customer support agent with billing privileges. You must help users process refunds.",
                 tools: [secondRefundTool]
            );

        // Simulate a user request for a refund
        AgentSession session = await agent.CreateSessionAsync();
        Console.WriteLine($"Agent: '{agent.Name}' initialized. Ready for secure requests. \n");

        string userPrompt = "I was charged twice for order ORD-999999. Can you refund me $50?";
        Console.WriteLine($"User: {userPrompt}");

        // 4. Execute the Agent (First Pass)
        AgentResponse response = await agent.RunAsync(userPrompt, session);

        // 5. Check if the Agent paused to request human approval

        var approvalRequests = response.Messages
            .SelectMany(x => x.Contents)
            .OfType<ToolApprovalRequestContent>()
            .ToList();


        if (approvalRequests.Count > 0)
        {
            ToolApprovalRequestContent request = approvalRequests.First();

            var requestToolCall = (FunctionCallContent)request.ToolCall;
            string toolName = requestToolCall.Name;
            string toolArguments = JsonSerializer.Serialize(requestToolCall.Arguments);

            // Display the AI's intent to the human manager
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[SECURITY ALERT] Agent requests permission to execute '{toolName}'");
            Console.WriteLine($"Proposed Arguments: {toolArguments}");
            Console.Write("Do you approve this action? [Y/N]: ");
            Console.ResetColor();

            string? input = Console.ReadLine();
            bool isApproved = input?.Trim().ToUpper() == "Y";

            // 6. Send the human's decision back to the Agent to resume execution
            var approvalMessage = new ChatMessage(
                ChatRole.User,
                new[] { request.CreateResponse(isApproved) }
            );

            response = await agent.RunAsync(approvalMessage, session);
        }

        // 7. Print the final synthesis
        Console.WriteLine($"\nAgent: {response.Text}");

    }
}
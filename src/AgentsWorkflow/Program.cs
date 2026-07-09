using AgentsWorkflow;
using AgentsWorkflow.Nodes;
using Microsoft.Agents.AI.Workflows;

public record TicketState(string UserQuery, string Category = "Unassigned", string FinalResolution = "");



internal class Program
{
    private static async Task Main(string[] args)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

        var agentFactory = new AgentsAzure(endpoint, deploymentName);

        var nodes = new TicketWorkflowNodes(agentFactory);

        var triageNode = nodes.TriageFunc.BindAsExecutor("TriageNode");
        var hardwareNode = nodes.HardwareFunc.BindAsExecutor("HardwareNode");
        var softwareNode = nodes.SoftwareFunc.BindAsExecutor("SoftwareNode");

        // 4. Build the Graph with Conditional Edges
        var workflow = TicketWorkflowFactory.Create(triageNode, hardwareNode, softwareNode);
        Console.WriteLine("--- Incoming Enterprise IT Ticket ---\n");
        var initialTicket = new TicketState("My laptop screen is flickering aggressively and the hinge feels loose.");

        // 5. Execute the Workflow Graph
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, initialTicket);

        TicketState? finalState = null;

        // Observe the events as the payload travels between the agents
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is ExecutorCompletedEvent executorComplete)
            {
                Console.WriteLine($"[System] -> Node '{executorComplete.ExecutorId}' completed.");

                // Cast Data to TicketState
                if (executorComplete.Data is TicketState ticketState)
                {
                    finalState = ticketState;
                    Console.WriteLine($"         State: Category='{ticketState.Category}', Resolution='{(string.IsNullOrEmpty(ticketState.FinalResolution) ? "(pending)" : "set")}'");
                }
            }
        }

        Console.WriteLine($"\n--- Final Resolution ---\n{finalState?.FinalResolution}");
    }
}

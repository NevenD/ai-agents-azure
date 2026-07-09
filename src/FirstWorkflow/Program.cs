
// 1. Event payload

using Microsoft.Agents.AI.Workflows;

public record CustomerPayload(string CompanyName, string Industry, bool IsValidated = false, string Status = "New");


internal class Program
{
    private static async Task Main(string[] args)
    {
        // 2a The validation Node
        Func<CustomerPayload, CustomerPayload> validateFunc = (payload) =>
        {
            Console.WriteLine($"[Validator]Validating customer: {payload.CompanyName}");
            bool isValid = !string.IsNullOrWhiteSpace(payload.CompanyName);
            string status = isValid ? "Validated" : "Rejected";
            return payload with { IsValidated = isValid, Status = status };
        };

        var validatorExecutor = validateFunc.BindAsExecutor("ValidationNode");

        // 2b The Enrichment Node
        Func<CustomerPayload, CustomerPayload> enrichFunc = (payload) =>
        {
            Console.WriteLine($"[Enricher]Appyling: {payload.Industry}  enterprise templates...");
            return payload with { Status = "Enriched" };
        };

        var enricherExecutor = enrichFunc.BindAsExecutor("EnrichmentNode");

        // 2c The Audit Node
        Func<CustomerPayload, CustomerPayload> auditFunc = (payload) =>
        {
            Console.WriteLine($"[Auditor]Logging final state to database. Final status: {payload.Status}");
            return payload;
        };

        var auditExecutor = auditFunc.BindAsExecutor("AuditNode");

        // constructing workflow grahps and to combine the nodes into a workflow
        var workflow = new WorkflowBuilder(validatorExecutor)
            // only enrich if valid
            .AddEdge<CustomerPayload>(validatorExecutor, enricherExecutor, condition: p => p?.IsValidated == true)
            // Conditional Edge: If invalid, skip the audit
            .AddEdge<CustomerPayload>(validatorExecutor, auditExecutor, condition: p => p?.IsValidated == false)
            // standard edge: Enrichment always flows to Audit
            .AddEdge(enricherExecutor, auditExecutor)
            .Build();


        Console.WriteLine($"--- Starting Workflow Execution ---\n");
        var initialPayload = new CustomerPayload("", "Healthcare");

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, initialPayload);

        // Listen to the stram to observe the nodes completing their work
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is ExecutorCompletedEvent executorComplete)
            {
                Console.WriteLine($"[System] -> Node {executorComplete.ExecutorId} completed successfully. \n");

            }
        }

        Console.WriteLine("--- Workflow Execution Completed ---");
    }
}

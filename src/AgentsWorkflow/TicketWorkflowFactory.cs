using Microsoft.Agents.AI.Workflows;

namespace AgentsWorkflow
{
    public class TicketWorkflowFactory
    {
        public static Workflow Create(ExecutorBinding triageNode, ExecutorBinding hardwareNode, ExecutorBinding softwareNode)
        {
            // 4. Build the Graph with Conditional Edges
            return new WorkflowBuilder(triageNode)
                .AddEdge<TicketState>(triageNode, hardwareNode, condition: state =>
                // If Triage says Hardware, route to the Hardware Agent
                    state?.Category.Contains("Hardware", StringComparison.OrdinalIgnoreCase) == true)
                // If Triage says Software, route to the Software Agent
                .AddEdge<TicketState>(triageNode, softwareNode, condition: state =>
                    state?.Category.Contains("Software", StringComparison.OrdinalIgnoreCase) == true)
                .Build();
        }
    }
}

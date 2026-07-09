using Microsoft.Agents.AI;

namespace AgentsWorkflow.Nodes
{
    internal class TicketWorkflowNodes
    {
        private readonly AgentsAzure _agentFactory;

        public TicketWorkflowNodes(AgentsAzure agentFactory)
        {
            _agentFactory = agentFactory;
        }

        // 1. Triage Node Definition
        // Triages the query and redirects the state to the appropraite node worker
        public Func<TicketState, TicketState> TriageFunc => state =>
        {
            Console.WriteLine($"[Triage] Analyzing ticket: '{state.UserQuery}'");

            // MAF function binders handle this synchronous context execution
            AgentResponse response = _agentFactory.TriageAgent.RunAsync(state.UserQuery).GetAwaiter().GetResult();

            string category = response.Text.Trim();
            Console.WriteLine($"[Triage] Decision: Routed to {category} Department.");

            return state with { Category = category };
        };

        // 2. Hardware Node Definition
        public Func<TicketState, TicketState> HardwareFunc => state =>
        {
            Console.WriteLine($"[Hardware Support] Generating resolution...");

            AgentResponse response = _agentFactory.HardwareAgent.RunAsync(state.UserQuery).GetAwaiter().GetResult();

            return state with { FinalResolution = response.Text };
        };

        // 3. Software Node Definition
        public Func<TicketState, TicketState> SoftwareFunc => state =>
        {
            Console.WriteLine($"[Software Support] Generating resolution...");

            AgentResponse response = _agentFactory.SoftwareAgent.RunAsync(state.UserQuery).GetAwaiter().GetResult();

            return state with { FinalResolution = response.Text };
        };
    }
}

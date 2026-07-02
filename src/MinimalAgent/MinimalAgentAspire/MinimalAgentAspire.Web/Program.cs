using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using MinimalAgentAspire.Web;
using MinimalAgentAspire.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_ENDPOINT' is not set");
var model = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

// Create an IChatClient using the Azure OpenAI client with Azure CLI credentials
IChatClient chatClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(model)
    .AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true)
    .Build();
builder.Services.AddSingleton(chatClient);

// 3. Define and Register the Agents
builder.AddAIAgent(
    name: "NetworkSupportAgent",
    instructions:
        """
        You are a Tier 1 IT Support Agent.
        Your answers must be concise, professional, and limited strictly to troubleshooting network and VPN connectivity.        
        Keep responses concise — 3-5 sentences per turn. Be direct and opinionated.        
        """,
    chatClient);

// 4. Register DevUI services
builder.AddDevUI();
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapDevUI();
app.MapOpenAIResponses();
app.MapOpenAIConversations();

// Map chat endpoint to trigger the agent
app.MapPost("/api/chat", async (ChatRequest request,
    [FromKeyedServices("NetworkSupportAgent")] AIAgent networkSupportAgent) =>
{
    var response = await networkSupportAgent.RunAsync(request.Message);
    return Results.Ok(new { response = response.Text });
});

app.Run();


internal record ChatRequest(string Message);
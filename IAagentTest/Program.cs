using IAagentTest.Agents;
using IAagentTest.Models;
using IAagentTest.Sessions;
using Microsoft.Agents.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<agentFactory>();
builder.Services.AddSingleton<InMemorySesionStore>();

var app = builder.Build();

app.MapPost("/chat", async (
    ChatRequest request,
    agentFactory agentFactory,
    InMemorySesionStore sessionStore) =>
{
    if (string.IsNullOrWhiteSpace(request.SessionId))
        return Results.BadRequest("SessionId es requerido.");

    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest("Message es requerido.");

    var agent = agentFactory.Agent;

    AgentSession session;
    if (sessionStore.TryGetSession(request.SessionId, out var serializedSession))
    {
        session = await agent.DeserializeSessionAsync(serializedSession);
    }
    else
    {
        session = await agent.CreateSessionAsync();
    }

    var response = await agent.RunAsync(request.Message, session);

    var updatedSession = await agent.SerializeSessionAsync(session);
    sessionStore.SaveSession(request.SessionId, updatedSession);

    return Results.Ok(new ChatResponse(request.SessionId, response.Text));
});

app.MapDelete("/chat/{sessionId}", (
    string sessionId,
    InMemorySesionStore sessionStore) =>
{
    sessionStore.DeleteSession(sessionId);
    return Results.NoContent();
});

app.Run();
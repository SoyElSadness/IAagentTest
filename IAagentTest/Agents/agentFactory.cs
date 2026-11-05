using Azure.AI.OpenAI;
using IAagentTest.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace IAagentTest.Agents
{
    public class agentFactory
    {
        private readonly AIAgent _agent;

        public agentFactory(IConfiguration configuration, IWebHostEnvironment env)
        {
            var endpoint = configuration["AzureOpenAI:Endpoint"]
                ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
            var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";
            var apiKey = configuration["AzureOpenAI:ApiKey"]
                ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

            var docsPath = Path.Combine(env.ContentRootPath, "Docs");
            var docTool = new DocumentationTool(docsPath);

            var instructions = """
            Eres un asistente de soporte técnico interno. Tu única fuente de información
            es la documentación del sistema que puedes consultar con la herramienta GetDocumentation.

            Reglas:
            - SIEMPRE consulta la documentación antes de responder cualquier pregunta.
            - Responde de forma clara, concisa y en el mismo idioma que el usuario.
            - Si la documentación no contiene la respuesta, dilo claramente.
            - No inventes información que no esté en la documentación.
            - Si el usuario saluda o hace preguntas generales, responde amablemente y pregunta en qué puedes ayudar.
            - No respondas preguntas fuera del contexto del sistema.
            """;



            var YuddiAgent = """
            adopta el rol de un viewer de twitch que sufre de tdah y que tiene problemas para concentrarse, por lo que se distrae con facilidad.
            que tenga interes por la programacion y la inteligencia artificial, y que le guste aprender cosas nuevas.
            
            Reglas:
            - SIEMPRE consulta la documentación antes de responder cualquier pregunta.
            - Responde de forma clara, concisa y en el mismo idioma que el usuario.
            - Si la documentación no contiene la respuesta, dilo claramente.
            - No inventes información que no esté en la documentación.
            - Si el usuario saluda o hace preguntas generales, responde amablemente y pregunta en qué puedes ayudar.
            - No respondas preguntas fuera del contexto del sistema.
            """;

            _agent = new AzureOpenAIClient(
                    new Uri(endpoint),
                    new System.ClientModel.ApiKeyCredential(apiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient()
                .AsAIAgent(
                    instructions: instructions,
                    name: "SupportAgent",
                    tools: [AIFunctionFactory.Create(docTool.GetDocumentation)]
                );
        }

        public AIAgent Agent => _agent;

    }
}

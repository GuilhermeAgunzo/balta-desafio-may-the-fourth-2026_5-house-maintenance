using System.ClientModel;
using HouseMaintenance.AI.Configuration;
using HouseMaintenance.AI.Prompts;
using HouseMaintenance.Application.Exceptions;
using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Core.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace HouseMaintenance.AI.Agents;

public sealed class HouseMaintenancePlanningAgent(
    IOptions<OpenAiAgentOptions> options,
    ILogger<HouseMaintenancePlanningAgent> logger) : IMaintenancePlanningAgent
{
    private const string AgentName = "HouseMaintenanceMissionPlanner";

    public async Task<MaintenancePlanDraft> GeneratePlanAsync(
        MaintenancePlanningContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Tasks.Count == 0)
        {
            throw new MaintenancePlannerUnavailableException("A missão chegou vazia à camada de IA.");
        }

        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new MaintenancePlannerUnavailableException(
                "Configure OpenAI:ApiKey antes de solicitar um plano de manutenção.");
        }

        try
        {
            AIAgent agent = CreateAgent(settings);
            AgentResponse<MaintenancePlanDraft> response = await agent.RunAsync<MaintenancePlanDraft>(
                HouseMaintenancePlanningPrompt.Build(context),
                cancellationToken: cancellationToken);

            return response.Result
                ?? throw new MaintenancePlannerUnavailableException("A IA não retornou um plano estruturado.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Falha de rede ao chamar o provedor OpenAI.");
            throw new MaintenancePlannerUnavailableException("Não foi possível alcançar o provedor OpenAI no momento.");
        }
        catch (ClientResultException exception)
        {
            logger.LogError(exception, "OpenAI rejeitou a execução do agente.");
            throw new MaintenancePlannerUnavailableException(
                $"O provedor OpenAI recusou a solicitação do agente: {exception.Message}");
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "A chamada ao agente excedeu o timeout configurado.");
            throw new MaintenancePlannerUnavailableException(
                $"A chamada ao agente excedeu o timeout configurado de {NormalizeTimeout(settings.TimeoutSeconds)} segundos. Ajuste OpenAI:TimeoutSeconds ou use um modelo mais rápido.");
        }
    }

    private static AIAgent CreateAgent(OpenAiAgentOptions settings)
    {
        OpenAIClient client = new(
            new ApiKeyCredential(settings.ApiKey!),
            new OpenAIClientOptions
            {
                NetworkTimeout = TimeSpan.FromSeconds(NormalizeTimeout(settings.TimeoutSeconds))
            });
        ChatClient chatClient = client.GetChatClient(settings.Model);

        return chatClient.AsAIAgent(
            name: AgentName,
            instructions: HouseMaintenancePlanningPrompt.Instructions);
    }

    private static int NormalizeTimeout(int timeoutSeconds) => Math.Clamp(timeoutSeconds, 30, 300);
}

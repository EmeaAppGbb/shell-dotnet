using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;



public class AGUIWorkflowAgent : DelegatingAIAgent
{
    public AGUIWorkflowAgent(AIAgent innerAgent) : base(innerAgent) { }

    public override Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.RunStreamingAsync(messages, thread, options, cancellationToken).ToAgentRunResponseAsync(cancellationToken);
    }

    public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var update in this.InnerAgent.RunStreamingAsync(messages, thread, options, cancellationToken).ConfigureAwait(false))
        {
            switch (update.RawRepresentation)
            {
                case WorkflowOutputEvent outputEvent:
                    {
                        yield return ExtractFunctionCallUpdate(update, outputEvent.Data);
                        //yield return CreateUpdateFromEvent(update, outputEvent.Data);
                        break;
                    }

                default:
                    yield return ExtractFunctionCallUpdate(update, update.RawRepresentation);
                    break;
            }
        }
    }

    private static AgentRunResponseUpdate ExtractFunctionCallUpdate(AgentRunResponseUpdate update, object? data)
    {
        IList<AIContent>? updatedContents = null;
        var content = data;
#pragma warning disable MEAI001 // Type is for evaluation purposes only
        if (content is FunctionApprovalRequestContent request)
        {
            updatedContents ??= [.. update.Contents];
            var functionCall = request.FunctionCall;
            var approvalId = request.Id;

            updatedContents.Add(new FunctionCallContent(
                callId: approvalId,
                name: functionCall.Name,
                arguments: functionCall.Arguments));
        }
        else if (content is TextContent textContent)
        {
            updatedContents ??= [.. update.Contents];
            updatedContents.Add(new TextContent(textContent.Text));
        }
#pragma warning restore MEAI001

        if (updatedContents is not null)
        {
            var chatUpdate = update.AsChatResponseUpdate();
            // Yield a tool call update that represents the approval request
            return new AgentRunResponseUpdate(new ChatResponseUpdate()
            {
                Role = chatUpdate.Role,
                Contents = updatedContents,
                MessageId = chatUpdate.MessageId,
                AuthorName = chatUpdate.AuthorName,
                CreatedAt = chatUpdate.CreatedAt,
                RawRepresentation = chatUpdate.RawRepresentation,
                ResponseId = chatUpdate.ResponseId,
                AdditionalProperties = chatUpdate.AdditionalProperties
            })
            {
                AgentId = update.AgentId,
                ContinuationToken = update.ContinuationToken
            };
        }
        return update;
    }
}
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
using Microsoft.Extensions.AI;

namespace agentic_api.Workflows;

/// <summary>
/// Helper class for creating human-in-the-loop approval requests in workflows.
/// </summary>
public static class ApprovalRequestHelper
{
    /// <summary>
    /// Creates a function approval request that prompts the user to approve an item.
    /// </summary>
    /// <param name="functionName">The name of the approval function to call</param>
    /// <param name="arguments">The arguments dictionary containing the data to be approved</param>
    /// <returns>A FunctionApprovalRequestContent that will prompt the user for approval</returns>
    public static FunctionApprovalRequestContent CreateApprovalRequest(
        string functionName,
        Dictionary<string, object?> arguments)
    {
        return new FunctionApprovalRequestContent(
            Guid.NewGuid().ToString(),
            new FunctionCallContent(
                functionName,
                functionName,
                arguments: arguments
            )
        );
    }
}

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace agentic_api.Workflows;

/// <summary>
/// Dummy workflow factory that creates a simple echo workflow for testing.
/// </summary>
public class DummyWorkflowFactory
{
    private readonly ILogger<DummyChatInputExecutor> _inputLogger;
    private readonly ILogger<TextGeneratorExecutor> _textGeneratorLogger;
    private readonly ILogger<ImageGeneratorExecutor> _imageGeneratorLogger;
    private readonly ILogger<FinalExecutor> _finalLogger;
    private readonly IChatClient _chatClient;
    private readonly IImageGenerator _imageGenerator;

    public DummyWorkflowFactory(
        ILogger<DummyChatInputExecutor> chatInputLogger,
        ILogger<TextGeneratorExecutor> textGeneratorLogger,
        ILogger<ImageGeneratorExecutor> imageGeneratorLogger,
        ILogger<FinalExecutor> finalLogger,
        IChatClient chatClient,
        IImageGenerator imageGenerator)
    {
        _inputLogger = chatInputLogger;
        _textGeneratorLogger = textGeneratorLogger;
        _imageGeneratorLogger = imageGeneratorLogger;
        _finalLogger = finalLogger;
        _chatClient = chatClient;
        _imageGenerator = imageGenerator;
    }

    public Workflow BuildWorkflow(string name)
    {
        // Create executors
        var chatInput = new DummyChatInputExecutor(_inputLogger);
        var textGenerator = new TextGeneratorExecutor(_textGeneratorLogger, _chatClient);
        var imageGenerator = new ImageGeneratorExecutor(_imageGeneratorLogger, _chatClient, _imageGenerator);
        var final = new FinalExecutor(_finalLogger);

        // Build workflow with conditional routing based on user input
        var workflowBuilder = new WorkflowBuilder(chatInput)
            .WithName(name)
            .AddSwitch(chatInput, switchBuilder =>
                switchBuilder.AddCase(GenerateText(), textGenerator)
                             .AddCase(GenerateImage(), imageGenerator)
                             .AddCase(FinalizeWorkflow(), final)
                             .WithDefault(textGenerator)
            )
            .WithOutputFrom(textGenerator)
            .WithOutputFrom(imageGenerator)
            .WithOutputFrom(final);

        return workflowBuilder.Build();
    }

    public static Func<UserInputEvent?, bool> GenerateText() => (input) =>
    {
        return input?.NextStep == DummyWorkflowSteps.GenerateText;
    };

    public static Func<UserInputEvent?, bool> GenerateImage() => (input) =>
    {
        return input?.NextStep == DummyWorkflowSteps.GenerateImage;
    };

    public static Func<UserInputEvent?, bool> FinalizeWorkflow() => (input) =>
    {
        return input?.NextStep == DummyWorkflowSteps.Finalize;
    };

}

/// <summary>
/// ChatInput executor that accepts List<ChatMessage> and TurnToken for dummy workflow.
/// </summary>
public sealed class DummyChatInputExecutor : Executor
{
    private readonly ILogger<DummyChatInputExecutor> _logger;

    public DummyChatInputExecutor(ILogger<DummyChatInputExecutor> logger) : base("DummyChatInput")
    {
        _logger = logger;
    }

    protected override Microsoft.Agents.AI.Workflows.RouteBuilder ConfigureRoutes(Microsoft.Agents.AI.Workflows.RouteBuilder routeBuilder) =>
        routeBuilder
            .AddHandler<List<ChatMessage>, UserInputEvent>(HandleChatMessagesAsync)
            .AddHandler<TurnToken, string>(HandleTurnTokenAsync);

    private async ValueTask<UserInputEvent> HandleChatMessagesAsync(
        List<ChatMessage> messages,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var lastUserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User);
        var approvalMessage = messages.LastOrDefault(m => m.Role == ChatRole.Tool);
        var functionResult = approvalMessage?.Contents.OfType<FunctionResultContent>().FirstOrDefault();

        var textApproved = functionResult?.Result?.ToString()?.Contains("text-approved");
        var imageApproved = functionResult?.Result?.ToString()?.Contains("image-approved");

        if (textApproved == true)
        {
            _logger.LogInformation("Text content approved by user.");
            return new UserInputEvent { Input = lastUserMessage?.Text ?? "Hello", NextStep = DummyWorkflowSteps.GenerateImage };
        }

        else if (imageApproved == true)
        {
            _logger.LogInformation("Image content approved by user.");
            return new UserInputEvent { Input = lastUserMessage?.Text ?? "Hello", NextStep = DummyWorkflowSteps.Finalize };
        }

        else
        {
            _logger.LogInformation("No approvals detected, proceeding to generate text content.");
            return new UserInputEvent { Input = lastUserMessage?.Text ?? "Hello", NextStep = DummyWorkflowSteps.GenerateText };
        }
    }

    private async ValueTask<string> HandleTurnTokenAsync(
        TurnToken turnToken,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var userInput = "Hello from TurnToken";

        _logger.LogInformation("Dummy Workflow started with TurnToken");

        return userInput;
    }
}

/// <summary>
/// Text generator executor that uses IChatClient to generate text content with human-in-the-loop approval.
/// </summary>
public sealed class TextGeneratorExecutor : Executor<UserInputEvent, AIContent>
{
    private readonly ILogger<TextGeneratorExecutor> _logger;
    private readonly AIAgent _agent;
    public TextGeneratorExecutor(ILogger<TextGeneratorExecutor> logger, IChatClient chatClient) : base("TextGenerator")
    {
        _logger = logger;

        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "TextGeneratorAgent",
            Instructions = "You are a helpful AI assistant that generates text content based on user input. Create clear, concise, and relevant responses. Keep your response under 4000 characters. Be brief and to the point."
        });
    }

    public override async ValueTask<AIContent> HandleAsync(
        UserInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Text generator executor received: {Input}", input.Input);
            _logger.LogInformation("Calling AI agent to generate text content");

            var options = new ChatOptions
            {
                MaxOutputTokens = 1000 // Approximately 4000 characters (1 token ≈ 4 chars)
            };

            var agentResponse = await _agent.RunAsync(
                new ChatMessage(ChatRole.User, input.Input), 
                options: new AgentRunOptions { AdditionalProperties = new() { ["ChatOptions"] = options } },
                cancellationToken: cancellationToken);

            var responseText = agentResponse.Text ?? "Generated text content";
            _logger.LogInformation($"AI agent responded with {responseText.Length} characters");
            
            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "approve_copyright_command",
                arguments: new Dictionary<string, object?>
                {
                    { "copyright", responseText }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in text generator executor: {Message}. Type: {Type}. StackTrace: {StackTrace}",
                ex.Message, ex.GetType().Name, ex.StackTrace);
            return new TextContent("Sorry, I encountered an error while generating text content.");
        }
    }
}


/// <summary>
/// Final executor that completes the workflow and returns a final response.
/// </summary>
public sealed class FinalExecutor : Executor<UserInputEvent, AIContent>
{
    private readonly ILogger<FinalExecutor> _logger;
    public FinalExecutor(ILogger<FinalExecutor> logger) : base("Final")
    {
        _logger = logger;
    }

    public override async ValueTask<AIContent> HandleAsync(
        UserInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Final executor received: {Input}", input.Input);
            _logger.LogInformation("Completing workflow");


            var responseText = "Workflow completed successfully!";
            _logger.LogInformation($"Final response: {responseText}");

            return new TextContent(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in final executor: {Message}. Type: {Type}. StackTrace: {StackTrace}",
                ex.Message, ex.GetType().Name, ex.StackTrace);
            return new TextContent("An error occurred while finalizing the workflow.");
        }
    }
}

/// <summary>
/// Image generator executor that creates images using IImageGenerator with human-in-the-loop approval.
/// </summary>
public sealed class ImageGeneratorExecutor : Executor<UserInputEvent, AIContent>
{
    private readonly ILogger<ImageGeneratorExecutor> _logger;
    private readonly IImageGenerator _imageGenerator;

    private readonly AIAgent _agent;
    public ImageGeneratorExecutor(ILogger<ImageGeneratorExecutor> logger, IChatClient chatClient, IImageGenerator imageGenerator) : base("ImageGenerator")
    {
        _logger = logger;
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
                {
                    Name = "ImagePromptAgent",
                    Instructions = "You are an expert in generating prompts for image generation models. Take the user input and create a safe, detailed, and descriptive image generation prompt."
                });
        _imageGenerator = imageGenerator;
    }

    public override async ValueTask<AIContent> HandleAsync(
        UserInputEvent input,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Image generator executor received: {Input}", input.Input);
            _logger.LogInformation("Calling AI agent to generate image prompt");

            var agentResponse = await _agent.RunAsync(new ChatMessage(ChatRole.User, input.Input), cancellationToken: cancellationToken);

            var imagePrompt = agentResponse.Text ?? "A scenic landscape";

            // Generate an image from a text prompt
            var options = new ImageGenerationOptions
            {
                MediaType = "image/png",
                ResponseFormat = ImageGenerationResponseFormat.Hosted
            };


            var response = await _imageGenerator.GenerateImagesAsync(imagePrompt, options);
            var dataContent = response.Contents.OfType<DataContent>().First();

            _logger.LogInformation($"Image was created at {dataContent.Uri}");
            return ApprovalRequestHelper.CreateApprovalRequest(
                functionName: "approve_design_command",
                arguments: new Dictionary<string, object?>
                {
                    { "design", dataContent.Uri }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in image generator executor: {Message}. Type: {Type}. StackTrace: {StackTrace}",
                ex.Message, ex.GetType().Name, ex.StackTrace);
            return new TextContent("Sorry, I encountered an error while generating the image.");
        }
    }
}

public class WorkflowState
{
    public bool TextApproved { get; set; }
    public bool ImageApproved { get; set; }

    public string? TextContent { get; set; }
    public string? ImageContent { get; set; }
}

public class UserInputEvent
{
    public required string Input { get; set; }
    public DummyWorkflowSteps NextStep { get; set; }
}

public class AgentRunResponse
{
    public required string Text { get; set; }
    public string? ImageUrl { get; set; }
}

public enum DummyWorkflowSteps
{
    GenerateText,
    GenerateImage,
    Finalize
}
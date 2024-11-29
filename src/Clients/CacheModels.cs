using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Serialization;
using BuildingBlocks.Utils;
using Clients.Contracts;
using Clients.Converters;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.Options;

namespace Clients;

public class CacheModels : ICacheModels
{
    private readonly ModelsInformationOptions _modelsInformation;
    private readonly ModelsOptions _modelOptions;
    private readonly LLMOptions _llmOptions;
    private readonly Dictionary<string, Model> _models = new();

    public CacheModels(
        IOptions<ModelsOptions> modelOptions,
        IOptions<ModelsInformationOptions> modelsInformation,
        IOptions<LLMOptions> llmOptions
    )
    {
        _modelsInformation = modelsInformation.Value;
        _modelOptions = modelOptions.Value;
        _llmOptions = llmOptions.Value;

        InitCache();
    }

    public Model? GetModel(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;

        if (_models.Count == 0)
        {
            throw new InvalidOperationException("Model cache is empty. Ensure models are initialized.");
        }

        if (_models.TryGetValue(modelName, out var model) || TryGetModelWithFallback(modelName, out model))
        {
            return model;
        }

        return null;
    }

    private bool TryGetModelWithFallback(string modelName, out Model model)
    {
        var parts = modelName.Split('/');

        if (
            parts.Length == 2
            && _models.TryGetValue(parts[1], out var fallbackModel)
            && fallbackModel.AIProvider.ToString() == parts[0]
        )
        {
            model = fallbackModel;
            return true;
        }

        model = null!;
        return false;
    }

    private void InitCache()
    {
        var modelsListJson = FilesUtilities.ReadEmbeddedResource(
            Assembly.GetExecutingAssembly(),
            $"{nameof(Clients)}.LLMs.models_information_list.json"
        );

        var modelsOptionsJson = FilesUtilities.ReadEmbeddedResource(
            Assembly.GetExecutingAssembly(),
            $"{nameof(Clients)}.LLMs.models_options.json"
        );

        var options = JsonObjectSerializer.NoCaseOptions;
        options.Converters.Add(new ModelInformationConverter());
        options.Converters.Add(new ModelOptionConverter());

        var predefinedModelOptions = JsonSerializer.Deserialize<Dictionary<string, ModelOption>>(
            modelsOptionsJson,
            options
        )!;

        var predefinedModelsInformation = JsonSerializer.Deserialize<Dictionary<string, ModelInformation>>(
            modelsListJson,
            options
        )!;

        foreach (
            var (originalName, predefinedModelInformation) in predefinedModelsInformation.Where(x => x.Value.Enabled)
        )
        {
            var predefinedModelOption = predefinedModelOptions.GetValueOrDefault(originalName);

            var overrideModelOption = _modelOptions.GetValueOrDefault(originalName);
            var overrideModelInformation = _modelsInformation.GetValueOrDefault(originalName);

            var model = new Model
            {
                Name = GetName(originalName),
                OriginalName = originalName,

                // Model Options
                CodeAssistType =
                    overrideModelOption?.CodeAssistType
                    ?? _llmOptions.CodeAssistType
                    ?? predefinedModelOption?.CodeAssistType
                    ?? CodeAssistType.Embedding,
                CodeDiffType =
                    overrideModelOption?.CodeDiffType
                    ?? _llmOptions.CodeDiffType
                    ?? predefinedModelOption?.CodeDiffType
                    ?? CodeDiffType.CodeBlockDiff,
                Threshold =
                    overrideModelOption?.Threshold ?? _llmOptions.Threshold ?? predefinedModelOption?.Threshold ?? 0.4m,
                Temperature =
                    overrideModelOption?.Temperature
                    ?? _llmOptions.Temperature
                    ?? predefinedModelOption?.Temperature
                    ?? 0.2m,
                ApiVersion = overrideModelOption?.ApiVersion ?? predefinedModelOption?.ApiVersion,
                BaseAddress = overrideModelOption?.BaseAddress ?? predefinedModelOption?.BaseAddress,
                DeploymentId = overrideModelOption?.DeploymentId ?? predefinedModelOption?.DeploymentId,

                // Model Information
                AIProvider =
                    overrideModelInformation?.AIProvider
                    ?? predefinedModelInformation.AIProvider
                    ?? throw new ArgumentException($"AI Provider not set for model {originalName}."),
                ModelType =
                    overrideModelInformation?.ModelType
                    ?? predefinedModelInformation.ModelType
                    ?? throw new ArgumentException($"Model Type not set for model {originalName}."),
                MaxTokens =
                    overrideModelInformation?.MaxTokens
                    ?? predefinedModelInformation.MaxTokens
                    ?? throw new ArgumentException($"Max tokens not set for model {originalName}."),
                MaxInputTokens =
                    overrideModelInformation?.MaxInputTokens
                    ?? predefinedModelInformation.MaxInputTokens
                    ?? throw new ArgumentException($"Max input tokens not set for model {originalName}."),
                MaxOutputTokens =
                    overrideModelInformation?.MaxOutputTokens
                    ?? predefinedModelInformation.MaxOutputTokens
                    ?? throw new ArgumentException($"Max output tokens not set for model {originalName}."),
                InputCostPerToken =
                    overrideModelInformation?.InputCostPerToken ?? predefinedModelInformation.InputCostPerToken,
                OutputCostPerToken =
                    overrideModelInformation?.OutputCostPerToken ?? predefinedModelInformation.OutputCostPerToken,
                OutputVectorSize =
                    overrideModelInformation?.OutputVectorSize ?? predefinedModelInformation.OutputVectorSize,
                Enabled = overrideModelInformation?.Enabled ?? predefinedModelInformation.Enabled,
                SupportsFunctionCalling =
                    overrideModelInformation?.SupportsFunctionCalling
                    ?? predefinedModelInformation.SupportsFunctionCalling,
                SupportsParallelFunctionCalling =
                    overrideModelInformation?.SupportsParallelFunctionCalling
                    ?? predefinedModelInformation.SupportsParallelFunctionCalling,
                SupportsVision = overrideModelInformation?.SupportsVision ?? predefinedModelInformation.SupportsVision,
                EmbeddingDimensions =
                    overrideModelInformation?.EmbeddingDimensions ?? predefinedModelInformation.EmbeddingDimensions,
                SupportsPromptCaching =
                    overrideModelInformation?.SupportsPromptCaching ?? predefinedModelInformation.SupportsPromptCaching,
            };

            _models[originalName] = model;
        }
    }

    private string GetName(string originalModelName)
    {
        var parts = originalModelName.Split('/');
        return parts.Length == 2 ? parts[1] : originalModelName;
    }
}

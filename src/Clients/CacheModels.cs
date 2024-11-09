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
    private readonly Dictionary<string, Model> _models = new();

    public CacheModels(IOptions<ModelsOptions> modelOptions, IOptions<ModelsInformationOptions> modelsInformation)
    {
        _modelsInformation = modelsInformation.Value;
        _modelOptions = modelOptions.Value;

        InitCache();
    }

    public Model GetModel(string modelName)
    {
        if (_models.Count == 0)
        {
            throw new InvalidOperationException("Model cache is empty. Ensure models are initialized.");
        }

        if (_models.TryGetValue(modelName, out var model) || TryGetModelWithFallback(modelName, out model))
        {
            return model;
        }

        throw new KeyNotFoundException($"Model '{modelName}' not found in the ModelCache.");
    }

    private bool TryGetModelWithFallback(string modelName, out Model model)
    {
        var parts = modelName.Split('/');
        if (
            parts.Length == 2
            && _models.TryGetValue(parts[1], out var fallbackModel)
            && fallbackModel.ModelInformation.AIProvider.ToString() == parts[0]
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

        foreach (var (originalName, information) in predefinedModelsInformation.Where(x => x.Value.Enabled))
        {
            var modelOption = predefinedModelOptions.GetValueOrDefault(originalName);
            var overrideModelOption = _modelOptions.GetValueOrDefault(originalName);
            var overrideModelInformation = _modelsInformation.GetValueOrDefault(originalName);

            var model = new Model
            {
                Name = GetName(originalName),
                OriginalName = originalName,
                ModelOption = new ModelOption
                {
                    CodeAssistType =
                        overrideModelOption?.CodeAssistType ?? modelOption?.CodeAssistType ?? CodeAssistType.Embedding,
                    CodeDiffType =
                        overrideModelOption?.CodeDiffType ?? modelOption?.CodeDiffType ?? CodeDiffType.CodeBlockDiff,
                    Threshold = overrideModelOption?.Threshold ?? modelOption?.Threshold ?? 0.4m,
                    Temperature = overrideModelOption?.Temperature ?? modelOption?.Temperature ?? 0.2m,
                },
                ModelInformation = new ModelInformation
                {
                    AIProvider = overrideModelInformation?.AIProvider ?? information.AIProvider,
                    ModelType = overrideModelInformation?.ModelType ?? information.ModelType,
                    MaxTokens = overrideModelInformation?.MaxTokens ?? information.MaxTokens,
                    MaxInputTokens = overrideModelInformation?.MaxInputTokens ?? information.MaxInputTokens,
                    MaxOutputTokens = overrideModelInformation?.MaxOutputTokens ?? information.MaxOutputTokens,
                    InputCostPerToken = overrideModelInformation?.InputCostPerToken ?? information.InputCostPerToken,
                    OutputCostPerToken = overrideModelInformation?.OutputCostPerToken ?? information.OutputCostPerToken,
                    OutputVectorSize = overrideModelInformation?.OutputVectorSize ?? information.OutputVectorSize,
                    Enabled = overrideModelInformation?.Enabled ?? information.Enabled,
                    SupportsFunctionCalling =
                        overrideModelInformation?.SupportsFunctionCalling ?? information.SupportsFunctionCalling,
                    SupportsParallelFunctionCalling =
                        overrideModelInformation?.SupportsParallelFunctionCalling
                        ?? information.SupportsParallelFunctionCalling,
                    SupportsVision = overrideModelInformation?.SupportsVision ?? information.SupportsVision,
                    EmbeddingDimensions =
                        overrideModelInformation?.EmbeddingDimensions ?? information.EmbeddingDimensions,
                    SupportsAudioInput = overrideModelInformation?.SupportsAudioInput ?? information.SupportsAudioInput,
                    SupportsAudioOutput =
                        overrideModelInformation?.SupportsAudioOutput ?? information.SupportsAudioOutput,
                    SupportsPromptCaching =
                        overrideModelInformation?.SupportsPromptCaching ?? information.SupportsPromptCaching,
                },
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

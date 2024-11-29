# AI Assist

> `Context Aware` AI coding assistant inside terminal to help in code development, code explanation, code refactor and review, bug fix and chat with supporting local and online language models

`AIAssist` is compatible with bellow AI Services: 
- [x] [OpenAI](https://platform.openai.com/docs/api-reference/introduction) through apis
- [x] [Azure AI Services](https://azure.microsoft.com/en-us/products/ai-services) through apis
- [x] [Ollama](https://ollama.com/) with using [ollama models](https://ollama.com/search) locally
- [ ] [Anthropic](https://docs.anthropic.com/en/api/getting-started) through apis
- [ ] [OpenRouter](https://openrouter.ai/docs/quick-start) through apis

> [!TIP]
> You can use ollama and its models that are more compatible with code like [deepseek-v2.5](https://ollama.com/library/deepseek-v2.5) or [qwen2.5-coder](https://ollama.com/library/qwen2.5-coder) locally. To use local models, you will need to run [Ollama](https://github.com/ollama/ollama) process first. For running ollama you can use [ollama docker](https://ollama.com/blog/ollama-is-now-available-as-an-official-docker-image) container.

> [!NOTE]
> Development of `vscode` and `jetbrains` plugins are in the plan and I will add them soon.

## Features

-   ✅ `Context Aware` ai code assistant through [ai embeddings](src/AIAssistant/Services/CodeAssistStrategies/EmbeddingCodeAssist.cs) which is based on Retrieval Augmented Generation (RAG) or [tree-sitter application summarization](src/AIAssistant/Services/CodeAssistStrategies/TreeSitterCodeAssistSummary.cs) to summarize application context and understanding by AI.
-   ✅ Support different result formats, like [Unified Diff Format](src/AIAssistant/Diff/UnifiedCodeDiffParser.cs), [Code Block Format](src/AIAssistant/Diff/CodeBlockDiffParser.cs) and [Search-Replace Format](src/AIAssistant/Diff/SearchReplaceParser.cs).
-   ✅ Code assistant for developing new features, finding bugs, refactor and review existing code base.
-   ✅ Chat mode for chatting with different local and online AI models through terminal.
-   ✅ Support local [ollama models](https://ollama.com/library) and [OpenAI](https://platform.openai.com/docs/models) and [Azure AI Service](https://ai.azure.com/explore/models) models.
-   ✅ Support multiple code languages like C#, Java, go,...
-   ✅ Syntax highlighting for showing code blocks and using `md format` for ai results with capability of `changing theme` like dracula theme or vscode light theme.
-   ✅ Defining a dedicated ignore file for AIAssist through a `.aiassistignore` for excluding files and folders that you want to exclude from code assist process and decreasing final token size.
-   ✅ Customize `configuration` through creating `aiassist-config.json` running directory for `aiassist` and a format like [predefined aiassist-config.json](./src/AIAssistant/aiassist-config.json).
-   ✅ Showing token usage count and calculated priced based on each model `input token` and `output token` price.
-   ✅ Customize models information with creating a customized models information through `ModelsInformationOptions` section in the `aiassist-config.json` and a format like [predefined models information](./src/Clients/LLMs/models_information_list.json).
-   ✅ Customize models options through `ModelsOptions` section in the `aiassist-config.json` and a format like [predefined models options](./src/Clients/LLMs/models_options.json).
-   ✅ Terminal main commands like `aiassist code`, `aiassist chat`, `aiassist explain` and support some internal commands like `:clear`, `:add-file`, `:clear-history`, `:token`, ...

## Get Started

AIAssist uses [Azure AI Services](https://azure.microsoft.com/en-us/products/ai-services) or [OpenAI](https://platform.openai.com/docs/api-reference/introduction) apis by default. For using `OpenAI` or `Azure AI` apis we need to have a `ApiKey`.

- To access `dotnet tool`, we need to install [latest .net sdk](https://dotnet.microsoft.com/en-us/download) first.
- Install `aiassist` with `dotnet tool install` and bellow command:

```bash
dotnet tool install --global AIAssist 
```

- For OpenAI If you don't have a API key you can [sign up](https://platform.openai.com/signup) in OpenAI and get a ApiKey.
- For Azure AI service you can [signup](https://azure.microsoft.com/en-us/products/ai-services) a azure account and get a AI model API key.
- After getting Api key we should set API key for chat and embedding models through environment variable or command options.
- Now got to `project directory` with `cd` command in terminal, For running `aiassist` and setting api key.

```bash
# Go to project directory
cd /to/project/directory
```

-   Set `Api Key` through `environment variable`:

Linux terminal:
```bash
export CHAT_MODEL_API_KEY=your-chat-api-key-here
export EMBEDDINGS_MODEL_API_KEY=your-embedding-api-key-here
```

Windows Powershell Terminal:
```powershell
$env:CHAT_MODEL_API_KEY=your-chat-api-key-here
$env:EMBEDDINGS_MODEL_API_KEY=your-embedding-api-key-here
```

-   Or set `Api Key` through `command option`:

```bash
aiassist code --chat-api-key your-chat-api-key-here  --embeddings-api-key your-embedding-api-key-here
```

-   If you are using AI models that need `ApiVersion`, `DeploymentId` and `BaseAddress` like Azure AI Service models, you can set them by environment variable or command options.
-   Set `ApiVersion`, `DeploymentId` and `BaseAddress` through`environment variable`:

Linux terminal:
```bash
export CHAT_BASE_ADDRESS=your-chat-base-address-here
export CHAT_API_VERSION=your-chat-api-version-here
export CHAT_DEPLOYMENT_ID=your-chat-deployment-id-here
export EMBEDDINGS_BASE_ADDRESS=your-embedding-base-address-here
export EMBEDDINGS_API_VERSION=your-embedding-api-version-here
export EMBEDDINGS_DEPLOYMENT_ID=your-embedding-deployment-id-here
```

Windows Powershell Terminal:
```powershell
$env:CHAT_BASE_ADDRESS=your-chat-base-address-here
$env:CHAT_API_VERSION=your-chat-api-version-here
$env:CHAT_DEPLOYMENT_ID=your-chat-deployment-id-here
$env:EMBEDDINGS_BASE_ADDRESS=your-embedding-base-address-here
$env:EMBEDDINGS_API_VERSION=your-embedding-api-version-here
$env:EMBEDDINGS_DEPLOYMENT_ID=your-embedding-deployment-id-here
```

-   Or set `ApiVersion`, `DeploymentId` and `BaseAddress` through `command option`:

```bash
aiassist code --chat-base-address your-chat-base-address-here --chat-api-version your-chat-api-version-here  --chat-deployment-id your-chat-deployment-id-here  --embeddings-base-address your-embeddings-base-address-here  --embeddings-api-version your-embeddings-api-version-here  --embeddings-deployment-id your-embeddings-deployment-id-here
```

-   Now run the ai assistant with `aiassist` command.

```bash
# run aiassist in code assistant mode.
aiassist code
```

## ⭐ Support

If you like feel free to ⭐ this repository, It helps out :)

Thanks a bunch for supporting me!

## Contribution

The application is in development status. You are feel free to submit a pull request or create an issue for any bugs or suggestions.

## License

The project is under [Apache-2.0 license](./LICENSE).

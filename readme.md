# AI Assist

> AI assistant for coding, chat, code explanation, review with supporting local and online language models.

`AIAssist` is compatible with [OpenAI](https://platform.openai.com/docs/api-reference/introduction) and [Azure AI Services](https://azure.microsoft.com/en-us/products/ai-services) through apis or [Ollama models](https://ollama.com/search) through [ollama engine](https://ollama.com/) locally.

> [!TIP]
> You can use ollama and its models that are more compatible with code like [deepseek-v2.5](https://ollama.com/library/deepseek-v2.5) or [qwen2.5-coder](https://ollama.com/library/qwen2.5-coder) locally. To use local models, you will need to run [Ollama](https://github.com/ollama/ollama) process first. For running ollama you can use [ollama docker](https://ollama.com/blog/ollama-is-now-available-as-an-official-docker-image) container.

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

## Get Started

AIAssist uses [Azure AI Services](https://azure.microsoft.com/en-us/products/ai-services) or [OpenAI](https://platform.openai.com/docs/api-reference/introduction) apis by default. For using `OpenAI` or `Azure AI` apis we need to have a `ApiKey`.

-   For OpenAI If you don't have a API key you can [sign up](https://platform.openai.com/signup) in OpenAI and get a ApiKey.
-   For Azure AI service you can [signup](https://azure.microsoft.com/en-us/products/ai-services) a azure account and get a AI model API key.
-   After getting Api key we should set API key for chat and embedding models through environment variable or command options.
-   Install `aiassist` with `dotnet tool install ` and bellow command:

```bash
TODO
```

-   Now got to `project directory` with `cd` command in terminal, For running `aiassist` and setting api key.

```bash
# Go to project directory
cd /to/project/directory
```

-   Setting `Api Key` through `environment variable`:

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

-   Or Setting `Api Key` through `command option`:

```bash
aiassist code --chat-api-key your-chat-api-key-here  --embeddings-api-key your-embedding-api-key-here
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

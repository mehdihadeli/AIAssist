namespace Clients;

public static class Constants
{
    public static class Ollama
    {
        public const string BaseAddress = "http://localhost:11434";

        public static class ChatModels
        {
            public const string Llama3_1 = "llama3.1";
            public const string Deepseek_Coder_V2 = "deepseek-coder-v2:16b-lite-instruct-q4_0";
        }

        public static class EmbeddingsModels
        {
            public const string Nomic_EmbedText = "nomic-embed-text";
            public const string Mxbai_Embed_Large = "mxbai-embed-large";
        }
    }

    public static class OpenAI
    {
        public const string BaseAddress = "https://api.openai.com";

        public static class ChatModels
        {
            public const string GPT4Mini = "gpt-4o-mini";
            public const string GPT3_5Turbo = "GPT-3.5 Turbo";
        }

        public static class EmbeddingsModels
        {
            public const string TextEmbeddingAda_002 = "text-embedding-ada-002";

            // https://openai.com/index/new-embedding-models-and-api-updates/
            public const string TextEmbedding3Large = "text-embedding-3-large";
            public const string TextEmbedding3Small = "text-embedding-3-small";
        }
    }

    public static class Anthropic
    {
        public const string BaseAddress = "https://api.anthropic.com";

        public static class ChatModels
        {
            public const string Claude_3_5_Sonnet = "claude-3-5-sonnet-20240620";
        }
    }
}

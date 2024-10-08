namespace Clients;

public static class Constants
{
    public static class Ollama
    {
        public static class ChatModels
        {
            public const string Llama3_1 = "llama3.1:latest";
        }

        public static class EmbeddingsModels
        {
            public const string Llama3_1 = "llama3.1:latest";
            public const string Nomic_EmbedText = "nomic-embed-text";
        }
    }

    public static class OpenAI
    {
        public static class ChatModels
        {
            public const string GPT4Mini = "gpt-4o-mini";
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
        public static class ChatModels
        {
            public const string Claude_3_5_Sonnet = "claude-3-5-sonnet-20240620";
        }

        public static class EmbeddingsModels
        {
            public const string Claude_3_5_Sonnet = "claude-3-5-sonnet-20240620";
        }
    }
}

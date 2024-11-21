using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.InMemoryVectorDatabase;
using Xunit;

namespace BuildingBlocks.UnitTests.InMemoryVectorDatabase;

//
// public class VectorDatabaseTests
// {
//     private readonly VectorContext _database = new();
//
//     [Fact]
//     public void CreateCollection_ShouldReturnNewCollection()
//     {
//         // Arrange
//         string collectionName = "docs";
//
//         // Act
//         VectorCollection collection = _database.GetCollection<Code>(collectionName);
//
//         // Assert
//         Assert.NotNull(collection);
//         Assert.Equal(collectionName, collection.Name);
//     }
//
//     [Fact]
//     public void AddDocument_ShouldStoreDocumentWithEmbeddingCorrectly()
//     {
//         // Arrange
//         var collection = _database.CreateOrGetCollection("docs");
//         var document = "Document 1";
//         var embedding = GenerateOllamaFakeEmbedding(document);
//         var metadata = new Dictionary<string, string> { { "source", "notion" } };
//
//         // Act
//         collection.AddDocuments(document, embedding, Guid.NewGuid(), metadata);
//
//         // Assert
//         var results = collection.QueryDocuments(GenerateOllamaFakeEmbedding("Document 1")).ToList();
//         Assert.Single(results);
//         Assert.Equal("Document 1", results[0].Text);
//     }
//
//     [Fact]
//     public void QueryDocuments_ByText_ShouldReturnRelevantDocuments()
//     {
//         // Arrange
//         var collection = _database.CreateOrGetCollection("docs");
//         var documents = new List<string>
//         {
//             "Llamas are members of the camelid family.",
//             "Llamas were first domesticated in Peru.",
//         };
//
//         foreach (var doc in documents)
//         {
//             var embedding = GenerateOllamaFakeEmbedding(doc);
//             collection.AddDocuments(
//                 doc,
//                 embedding,
//                 Guid.NewGuid(),
//                 new Dictionary<string, string> { { "source", "source1" } }
//             );
//         }
//
//         // Act
//         var results = collection
//             .QueryDocuments(GenerateOllamaFakeEmbedding("What animals are llamas related to?"), nResults: 1)
//             .ToList();
//
//         // Assert
//         Assert.Single(results);
//         Assert.Equal("Llamas were first domesticated in Peru.", results[0].Text);
//     }
//
//     [Fact]
//     public void QueryDocuments_ByText_WithMetadataFilter_ShouldReturnFilteredResults()
//     {
//         // Arrange
//         var collection = _database.CreateOrGetCollection("docs");
//         var documents = new List<string>
//         {
//             "Llamas are members of the camelid family.",
//             "Llamas are domesticated animals.",
//         };
//
//         foreach (var doc in documents)
//         {
//             var embedding = GenerateOllamaFakeEmbedding(doc);
//             collection.AddDocuments(
//                 doc,
//                 embedding,
//                 Guid.NewGuid(),
//                 new Dictionary<string, string> { { "source", "source1" } }
//             );
//         }
//
//         // Act
//         var results = collection
//             .QueryDocuments(
//                 GenerateOllamaFakeEmbedding("Llamas"),
//                 nResults: 2,
//                 metadataFilter: new Dictionary<string, string> { { "source", "source1" } }
//             )
//             .ToList();
//
//         // Assert
//         Assert.Equal(2, results.Count);
//         Assert.Contains(results, r => r.Text == "Llamas are members of the camelid family.");
//         Assert.Contains(results, r => r.Text == "Llamas are domesticated animals.");
//     }
//
//     [Fact]
//     public void QueryDocuments_ByEmbedding_ShouldReturnRelevantDocuments()
//     {
//         // Arrange
//         var collection = _database.CreateOrGetCollection("docs");
//         var documents = new List<string>
//         {
//             "Llamas are members of the camelid family.",
//             "Llamas were first domesticated in Peru.",
//         };
//
//         foreach (var doc in documents)
//         {
//             var embedding = GenerateOllamaFakeEmbedding(doc);
//             collection.AddDocuments(
//                 doc,
//                 embedding,
//                 Guid.NewGuid(),
//                 new Dictionary<string, string> { { "source", "source2" } }
//             );
//         }
//
//         // Act
//         var queryEmbedding = GenerateOllamaFakeEmbedding("Llamas are closely related to camelid.");
//         var results = collection.QueryDocuments(queryEmbedding, nResults: 1).ToList();
//
//         // Assert
//         Assert.Single(results);
//         Assert.Equal("Llamas are members of the camelid family.", results[0].Text);
//     }
//
//     [Fact]
//     public void ListCollections_ShouldReturnAllCollections()
//     {
//         // Arrange
//         _database.CreateOrGetCollection("docs");
//         _database.CreateOrGetCollection("another_collection");
//
//         // Act
//         var collections = _database.ListCollections();
//
//         // Assert
//         Assert.Equal(2, collections.Count);
//         Assert.Contains("docs", collections);
//         Assert.Contains("another_collection", collections);
//     }
//
//     // Helper method to generate fake embeddings
//     private double[] GenerateOllamaFakeEmbedding(string text)
//     {
//         // Use SHA256 to generate a hash from the text and convert it to a double array.
//         using var sha256 = SHA256.Create();
//
//         var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
//
//         // Normalize the hash into a double array of size 128
//         var embedding = new double[128];
//
//         for (int i = 0; i < embedding.Length; i++)
//         {
//             if (i < hash.Length)
//             {
//                 // Normalize each byte to a value between 0 and 1
//                 embedding[i] = hash[i] / 255.0;
//             }
//             else
//             {
//                 // Fill the rest with zeros if the hash is shorter than the embedding size
//                 embedding[i] = 0;
//             }
//         }
//
//         return embedding;
//     }
// }

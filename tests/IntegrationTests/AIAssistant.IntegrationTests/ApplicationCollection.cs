namespace AIAssistant.IntegrationTests;

[CollectionDefinition(Name)]
public class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
    public const string Name = "ApplicationCollection";
}

namespace AccessibleSchoolReports.UnitTests.Security;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SecurityCollection : ICollectionFixture<SecurityWebApplicationFactory>
{
    public const string Name = "Security";
}

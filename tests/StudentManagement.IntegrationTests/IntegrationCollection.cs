using StudentManagement.IntegrationTests.Fixtures;

namespace StudentManagement.IntegrationTests;

/// <summary>
/// Marks all integration tests that share a single set of containers.
/// One DatabaseFixture instance is started once and reused across all
/// test classes decorated with [Collection("Integration")].
/// </summary>
[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<DatabaseFixture> { }

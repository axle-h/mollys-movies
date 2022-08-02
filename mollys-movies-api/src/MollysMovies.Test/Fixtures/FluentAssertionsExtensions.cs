using System.IO.Abstractions.TestingHelpers;

namespace MollysMovies.Test.Fixtures;

public static class FluentAssertionsExtensions
{
    public static MockFileSystemAssertions Should(this MockFileSystem instance) => new(instance);
}
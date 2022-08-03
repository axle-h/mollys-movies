using System.IO.Abstractions.TestingHelpers;

namespace MollysMovies.FakeData.FileSystem;

public static class FluentAssertionsExtensions
{
    public static MockFileSystemAssertions Should(this MockFileSystem instance) => new(instance);
}
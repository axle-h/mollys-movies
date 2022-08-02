using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using FluentAssertions.Execution;

namespace MollysMovies.FakeData.FileSystem;

public class MockFileSystemAssertions
{
    private readonly MockFileSystem _subject;

    public MockFileSystemAssertions(MockFileSystem subject)
    {
        _subject = subject;
    }

    public AndConstraint<MockFileSystemAssertions> ContainFile(string rawPath, string content, string because = "",
        params object[] becauseArgs)
    {
        var path = FixPath(rawPath);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => _subject.AllFiles.ToList())
            .ForCondition(files => files.Contains(path))
            .FailWith(
                "Expected {context:filesystem} to contain file {0}{reason}, but no file exists.",
                path)
            .Then
            .Given(_ => _subject.GetFile(path).TextContents)
            .ForCondition(observedContent => observedContent == content)
            .FailWith(
                "Expected file at {0} to contain {1}{reason}, but content is {2}.",
                _ => path, _ => content, x => x);

        return new AndConstraint<MockFileSystemAssertions>(this);
    }

    public AndConstraint<MockFileSystemAssertions> ContainEmptyDirectory(string rawPath, string because = "",
        params object[] becauseArgs)
    {
        var path = FixPath(rawPath);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => _subject.GetFile(path))
            .ForCondition(file => file?.IsDirectory ?? false)
            .FailWith(
                "Expected {context:filesystem} to contain directory {0}{reason}, but not exists.",
                path)
            .Then
            .Given(_ => _subject.AllFiles.Where(x => x.StartsWith(path)))
            .ForCondition(files => !files.Any())
            .FailWith(
                "Expected directory at {0} to be empty{reason}, but contains files {1}.",
                _ => path, x => string.Join(", ", x));

        return new AndConstraint<MockFileSystemAssertions>(this);
    }

    private static string FixPath(string path) =>
        Path.GetFullPath(path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
}
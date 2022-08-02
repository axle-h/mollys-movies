using System.Collections.Concurrent;
using System.IO.Abstractions;
using FluentValidation;

namespace MollysMovies.Common.Validation;

public static class FluentValidationExtensions
{
    private static readonly ConcurrentDictionary<string, bool> DirectoryExistsCache = new();
    private static readonly ConcurrentDictionary<string, bool> WriteableDirectoryCache = new();

    public static IRuleBuilderOptions<T, string> DirectoryExists<T>(this IRuleBuilder<T, string> ruleBuilder,
        IFileSystem fileSystem) =>
        ruleBuilder.Must(path => DirectoryExistsCache.GetOrAdd(fileSystem.Path.GetFullPath(path), fileSystem.Directory.Exists))
            .WithMessage("{PropertyName} must be an existing directory but {PropertyValue} is not");

    public static IRuleBuilderOptions<T, string> WriteableDirectory<T>(this IRuleBuilder<T, string> ruleBuilder,
        IFileSystem fileSystem) =>
        ruleBuilder.Must(path => WriteableDirectoryCache.GetOrAdd(path, _ => IsWritableDirectory(path, fileSystem)))
            .WithMessage("{PropertyName} must be a writable directory but {PropertyValue} is not");

    private static bool IsWritableDirectory(string path, IFileSystem fileSystem)
    {
        var tmpFile = Path.Join(path, Guid.NewGuid().ToString());

        try
        {
            fileSystem.File.WriteAllBytes(tmpFile, new byte[] {0xff});
            fileSystem.File.Delete(tmpFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
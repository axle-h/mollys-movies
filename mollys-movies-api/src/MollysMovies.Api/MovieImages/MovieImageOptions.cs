using System.IO.Abstractions;
using FluentValidation;
using MollysMovies.Common.Validation;

namespace MollysMovies.Api.MovieImages;

public class MovieImageOptions
{
    public string Path { get; set; } = "";
}

public class MovieImageOptionsValidator : AbstractValidator<MovieImageOptions>
{
    public MovieImageOptionsValidator(IFileSystem fileSystem)
    {
        RuleFor(x => x.Path)
            .NotNull().NotEmpty()
            .DirectoryExists(fileSystem);
    }
}
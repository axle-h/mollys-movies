using System.Collections.Generic;
using System.IO.Abstractions;
using FluentValidation;
using MollysMovies.Common.Validation;

namespace MollysMovies.Api.Transmission;

public class TransmissionOptions
{
    public List<string> Trackers { get; set; } = new();
}

public class TransmissionOptionsValidator : AbstractValidator<TransmissionOptions>
{
    public TransmissionOptionsValidator(IFileSystem fileSystem)
    {
        RuleFor(x => x!.Trackers).NotEmpty().ForEach(rb => rb.NotNull().NotEmpty());
    }
}
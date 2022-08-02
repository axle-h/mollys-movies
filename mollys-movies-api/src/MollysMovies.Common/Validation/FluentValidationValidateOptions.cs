using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MollysMovies.Common.Validation;

public class FluentValidationValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly string _name;
    private readonly IServiceProvider _provider;

    public FluentValidationValidateOptions(string name, IServiceProvider provider)
    {
        _name = name;
        _provider = provider;
    }

    public ValidateOptionsResult Validate(string name, TOptions options)
    {
        if (_name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        using var scope = _provider.CreateScope();
        var validatorFactory = scope.ServiceProvider.GetRequiredService<IValidatorFactory>();

        var validator = validatorFactory.GetValidator<TOptions>() ??
                        throw new Exception($"no validator found for {typeof(TOptions)}");
        var result = validator.Validate(options);
        return result.IsValid
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(result.Errors.Select(x => x.ErrorMessage));
    }
}

public static class OptionsBuilderDataAnnotationsExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidator<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(p =>
            new FluentValidationValidateOptions<TOptions>(optionsBuilder.Name, p));
        return optionsBuilder;
    }
}
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MollysMovies.Api.Common.Exceptions;

public class ApiExceptionFilter : ExceptionFilterAttribute
{
    /// <summary>
    ///     Called when an exception is thrown by the executing action.
    /// </summary>
    /// <param name="context">The context.</param>
    public override void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case EntityNotFoundException e:
                context.ExceptionHandled = true;
                context.ModelState.AddModelError(string.Empty, e.Message);
                context.Result = new NotFoundObjectResult(new SerializableError(context.ModelState));
                break;

            case ValidationException e:
                HandleBadRequest(context, e.Errors);
                break;

            case BadRequestException e:
                HandleBadRequest(context, e.Validation.Errors);
                break;
        }
    }

    private static void HandleBadRequest(ExceptionContext context, IEnumerable<ValidationFailure> errors)
    {
        context.ExceptionHandled = true;
        foreach (var error in errors)
        {
            context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        context.Result = new BadRequestObjectResult(context.ModelState);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MolliesMovies.Common.Validation
{
    /// <summary>
    /// Specifies that the model state should be validated and when invalid a bad request response should be created.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // Invalid model, explicitly set a HTTP 400 bad request response.
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
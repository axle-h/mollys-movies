using Microsoft.AspNetCore.Mvc;

namespace MolliesMovies.Common.Routing
{
    /// <summary>
    /// Defines a route to an API controller.
    /// </summary>
    public abstract class ApiRouteAttribute : RouteAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiRouteAttribute" /> class.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        /// <param name="version">The version.</param>
        /// <param name="apiPath">The API path.</param>
        protected ApiRouteAttribute(string template, int version, string apiPath)
            : base(GetTemplate(apiPath, template, version))
        {
        }

        private static string GetTemplate(string apiPath, string template, int version)
        {
            var templateBase = $"/{apiPath}/v{version}";
            return string.IsNullOrEmpty(template)
                ? templateBase + "/[controller]"
                : $"{templateBase}/{template.TrimStart('/')}";
        }
    }
}
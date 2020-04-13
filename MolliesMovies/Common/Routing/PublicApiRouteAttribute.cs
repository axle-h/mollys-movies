namespace MolliesMovies.Common.Routing
{
    /// <summary>
    /// Defines a route to an API meant to be consumed by publicly.
    /// </summary>
    public class PublicApiRouteAttribute : ApiRouteAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicApiRouteAttribute" /> class.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        /// <param name="version">The version.</param>
        public PublicApiRouteAttribute(string template = null, int version = 1)
            : base(template, version, RouteConstants.PublicApiPath)
        {
            
        }
    }
}
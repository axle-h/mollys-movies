using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MolliesMovies.Common.Routing
{
    public static class OpenApiExtensions
    {
         public static string DtoSafeFriendlyId(this Type type) =>
            type.DefaultSchemaIdSelector().Replace("Dto", string.Empty);

        public static string GetOperationId(this ApiDescription apiDescription) =>
            apiDescription.ActionDescriptor switch
            {
                ControllerActionDescriptor cad => cad.ActionName,
                _ => apiDescription.ActionDescriptor.AttributeRouteInfo?.Name
            };

        public static bool IsApi(this ApiDescription api, string name)
        {
            var nameMatch = Regex.Match(name, @"^(.+)-v\d+$");
            if (!nameMatch.Success)
            {
                return false;
            }

            var versionStripped = nameMatch.Groups[1].Value.Replace("_", "/");
            return api.RelativePath.StartsWith(versionStripped);
        }

        public static SwaggerUIOptions PublicApi(this SwaggerUIOptions options, int version) =>
            options.Api(RouteConstants.PublicApiPath, version);

        private static string DefaultSchemaIdSelector(this Type modelType)
        {
            if (!modelType.IsConstructedGenericType)
            {
                return modelType.Name;
            }

            return modelType
                       .GetGenericArguments()
                       .Select(DefaultSchemaIdSelector)
                       .Aggregate((previous, current) => previous + current)
                   + modelType.Name.Split('`').First();
        }

        private static SwaggerUIOptions Api(this SwaggerUIOptions options, string routePath, int version)
        {
            var name = RouteConstants.GetApiName(routePath, version);
            options.SwaggerEndpoint($"/swagger/{name}/swagger.json", name);
            return options;
        }
    }
}
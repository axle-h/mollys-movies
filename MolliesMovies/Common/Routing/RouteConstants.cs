namespace MolliesMovies.Common.Routing
{
    public static class RouteConstants
    {
        public const string PublicApiPath = "api";
        
        public static string PublicApi(int version) => GetApiName(PublicApiPath, version);
        
        public static string GetApiName(string routePath, int version) => $"{routePath.Replace("/", "_")}-v{version}";
    }
}
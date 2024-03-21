namespace MakeMovies.Api;

public static class ConfigurationExtensions
{
    public static string DefaultSchemaIdSelector(this Type modelType)
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
}
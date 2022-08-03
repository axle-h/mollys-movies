using System;
using System.Linq;
using System.Text.Json;

namespace MollysMovies.Api.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(object id, Type type, string? condition = null)
        : base(GetMessage(id, type, condition))
    {
        Id = id;
        Type = type;
        Condition = condition;
    }

    public object Id { get; }

    public Type Type { get; }

    public string? Condition { get; }

    public static EntityNotFoundException Of<TEntity>(object id, string? condition = null) =>
        new(id, typeof(TEntity), condition);

    private static string GetMessage(object id, Type type, string? condition)
    {
        var (idString, idName) = id switch
        {
            string s => (s, "id"),
            int i => (i.ToString(), "id"),
            Guid g => (g.ToString(), "uuid"),
            _ => (JsonSerializer.Serialize(id), "keys")
        };
        var tokens = new[] {$"cannot find {type.Name} with {idName} {idString}", condition};
        return string.Join(", ", tokens.Where(x => x is not null));
    }
}
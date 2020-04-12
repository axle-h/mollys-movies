using System;
using Newtonsoft.Json;

namespace MolliesMovies.Common.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(object id, Type type) : base($"cannot find {type} with id {GetIdString(id)}")
        {
            Id = id;
            Type = type;
        }

        public object Id { get; }
        
        public Type Type { get; }
        
        public static EntityNotFoundException Of<TEntity>(object id) =>
            new EntityNotFoundException(id, typeof(TEntity));

        private static string GetIdString(object id) =>
            id switch
            {
                string s => s,
                int i => i.ToString(),
                Guid g => g.ToString(),
                _ => JsonConvert.SerializeObject(id)
            };
    }
}
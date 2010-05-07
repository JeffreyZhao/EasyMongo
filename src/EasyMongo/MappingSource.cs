using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public class MappingSource
    {
        private Dictionary<Type, EntityMapper> m_mappers;

        public MappingSource(IEnumerable<IEntityDescriptor> descriptors)
        {
            this.m_mappers = descriptors.ToDictionary(
                d => d.Type,
                d => new EntityMapper(d));
        }

        internal EntityMapper GetEntityMapper<T>()
        {
            var type = typeof(T);
            EntityMapper value;
            return this.m_mappers.TryGetValue(type, out value) ? value : null;
        }
    }
}

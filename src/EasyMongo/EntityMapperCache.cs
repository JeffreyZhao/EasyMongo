using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    internal class EntityMapperCache<TEntity> : ReadWriteCache<IEntityDescriptor<TEntity>, EntityMapper<TEntity>>
        where TEntity : class, new()
    {
        protected override EntityMapper<TEntity> Create(IEntityDescriptor<TEntity> key)
        {
            return new EntityMapper<TEntity>(key);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MongoDB.Driver;

namespace EasyMongo
{
    internal class EntityState : Dictionary<IPropertyDescriptor, object>
    {
        public EntityState(EntityMapper mapper)
        {
            this.Mapper = mapper;
        }

        public EntityMapper Mapper { get; private set; }
    }
}

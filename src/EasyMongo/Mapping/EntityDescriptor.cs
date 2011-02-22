using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace EasyMongo.Mapping
{
    internal class EntityDescriptor<T> : IEntityDescriptor<T>
    {
        public EntityDescriptor(string collectionName, IList<IPropertyDescriptor> properties)
        {
            this.CollectionName = collectionName;
            this.Properties = new ReadOnlyCollection<IPropertyDescriptor>(properties);
        }

        public string CollectionName { get; private set; }

        public ReadOnlyCollection<IPropertyDescriptor> Properties { get; private set; }
    }
}

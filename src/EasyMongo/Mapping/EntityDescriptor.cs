using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace EasyMongo.Mapping
{
    internal class EntityDescriptor : IEntityDescriptor
    {
        public Type Type { get; set; }

        public string CollectionName { get; set; }

        public ReadOnlyCollection<IPropertyDescriptor> Properties { get; set; }
    }
}

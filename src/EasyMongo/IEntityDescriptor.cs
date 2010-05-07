using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace EasyMongo
{
    public interface IEntityDescriptor
    {
        Type Type { get; }

        string CollectionName { get; }

        ReadOnlyCollection<IPropertyDescriptor> Properties { get; }
    }
}

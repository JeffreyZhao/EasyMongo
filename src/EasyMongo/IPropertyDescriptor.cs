using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using EasyMongo.Types;

namespace EasyMongo
{
    public interface IPropertyDescriptor
    {
        PropertyInfo Property { get; }

        string Name { get; }

        bool HasDefaultValue { get; }

        object GetDefaultValue();

        ITypeProcessor GetTypeProcessor();

        bool IsIdentity { get; }

        ReadOnlyCollection<PropertyInfo> ChangeWithProperties { get; }

        bool IsVersion { get; }
    }
}

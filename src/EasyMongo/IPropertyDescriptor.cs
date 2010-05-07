using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace EasyMongo
{
    public interface IPropertyDescriptor
    {
        PropertyInfo Property { get; }

        string Name { get; }

        bool HasDefaultValue { get; }

        object DefaultValue { get; }

        bool IsIdentity { get; }

        ReadOnlyCollection<PropertyInfo> ChangeWithProperties { get; }
    }
}

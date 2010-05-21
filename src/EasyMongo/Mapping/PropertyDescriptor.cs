using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace EasyMongo.Mapping
{
    internal class PropertyDescriptor : IPropertyDescriptor
    {
        public PropertyInfo Property { get; set; }

        public string Name { get; set; }

        public bool HasDefaultValue { get; set; }

        public Func<object> DefaultValueFactory { get; set; }

        public bool IsIdentity { get; set; }

        public ReadOnlyCollection<PropertyInfo> ChangeWithProperties { get; set; }

        public object GetDefaultValue()
        {
            return this.DefaultValueFactory.Invoke();
        }
    }
}

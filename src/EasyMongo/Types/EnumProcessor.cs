using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace EasyMongo.Types
{
    internal class EnumProcessor : BasicProcessor
    {
        public EnumProcessor(PropertyInfo property)
        {
            this.Property = property;
        }

        public PropertyInfo Property { get; private set; }

        public override object ToDocumentValue(object value)
        {
            if (this.Property.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                return value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
            }
            else
            {
                return value.ToString();
            }
        }

        public override object FromDocumentValue(object docValue)
        {
            if (this.Property.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                var itemNames = ((IEnumerable)docValue).Cast<string>().ToArray();
                return Enum.Parse(this.Property.PropertyType, String.Join(",", itemNames));
            }
            else
            {
                return Enum.Parse(this.Property.PropertyType, ((string)docValue));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace EasyMongo.Types
{
    public class EnumType : IType
    {
        public object ToDocumentValue(PropertyInfo propertyInfo, object value)
        {
            if (propertyInfo.IsDefined(typeof(FlagsAttribute), false))
            {
                return value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
            }
            else
            {
                return value.ToString();
            }
        }

        public object FromDocumentValue(PropertyInfo propertyInfo, object docValue)
        {
            if (propertyInfo.IsDefined(typeof(FlagsAttribute), false))
            {
                var itemNames = ((IEnumerable)docValue).Cast<string>().ToArray();
                return Enum.Parse(propertyInfo.PropertyType, String.Join(",", itemNames));
            }
            else
            {
                return Enum.Parse(propertyInfo.PropertyType, ((string)docValue));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using MongoDB.Driver;

namespace EasyMongo.Types
{
    public class ArrayType : IType
    {
        public object ToDocumentValue(PropertyInfo propertyInfo, object value)
        {
            return ((IList)value).Cast<object>().ToArray();
        }

        public object FromDocumentValue(PropertyInfo propertyInfo, object docValue)
        {
            var array = (IList)Activator.CreateInstance(propertyInfo.PropertyType);
            if (docValue is Document) return array;

            foreach (var item in (object[])docValue) array.Add(item);
            return array;
        }
    }
}

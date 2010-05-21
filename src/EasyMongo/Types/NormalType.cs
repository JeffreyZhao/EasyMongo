using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace EasyMongo.Types
{
    public class NormalType : IType
    {
        public object ToDocumentValue(PropertyInfo propertyInfo, object value)
        {
            return value;
        }

        public object FromDocumentValue(PropertyInfo propertyInfo, object docValue)
        {
            return docValue;
        }
    }
}

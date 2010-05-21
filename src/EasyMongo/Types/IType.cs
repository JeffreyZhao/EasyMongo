using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace EasyMongo.Types
{
    internal interface IType
    {
        object ToDocumentValue(PropertyInfo propertyInfo, object value);

        object FromDocumentValue(PropertyInfo propertyInfo, object docValue);
    }
}

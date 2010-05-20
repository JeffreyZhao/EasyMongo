using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Reflection;

namespace EasyMongo
{
    internal interface IPropertyPredicate
    {
        PropertyInfo Property { get; }

        void Fill(IPropertyPredicateOperator mapper, Document doc);
    }
}

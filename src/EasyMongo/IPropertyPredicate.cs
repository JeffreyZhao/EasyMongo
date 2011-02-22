using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Reflection;
using MongoDB.Bson;

namespace EasyMongo
{
    internal interface IPropertyPredicate
    {
        PropertyInfo Property { get; }

        void Fill(IPropertyPredicateOperator opr, QueryDocument doc);
    }
}

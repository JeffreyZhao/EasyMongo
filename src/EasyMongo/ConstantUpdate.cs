using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using MongoDB.Driver;
using System.Diagnostics;
using MongoDB.Bson;

namespace EasyMongo
{
    internal class ConstantUpdate : IPropertyUpdate
    {
        public static ConstantUpdate Create(PropertyInfo property, ConstantExpression constantExpr)
        {
            Debug.Assert(property != null, "property should not be null");
            Debug.Assert(constantExpr != null, "constantExpr should not be null");

            return new ConstantUpdate(property, constantExpr.Value);
        }

        public ConstantUpdate(PropertyInfo property, object value)
        {
            this.Property = property;
            this.Value = value;
        }

        public PropertyInfo Property { get; private set; }
        public object Value { get; private set; }

        public void Fill(IPropertyUpdateOperator optr, BsonDocument doc)
        {
            optr.PutConstantUpdate(doc, this.Value);
        }
    }
}

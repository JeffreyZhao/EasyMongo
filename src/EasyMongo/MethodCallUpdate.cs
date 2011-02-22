using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics;
using MongoDB.Driver;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;
using EasyMongo.Expressions;
using MongoDB.Bson;

namespace EasyMongo
{
    internal class MethodCallUpdate : IPropertyUpdate
    {
        public static MethodCallUpdate Create(PropertyInfo property, MethodCallExpression callExpr)
        {
            Debug.Assert(property != null, "property should not be null");
            Debug.Assert(callExpr != null, "callExpr should not be null");

            var method = GetSupportedMethod(callExpr);
            var argument = GetSupprotedArgument(callExpr);

            return new MethodCallUpdate(property, method, argument);
        }

        private static MethodInfo GetSupportedMethod(MethodCallExpression callExpr)
        {
            if (callExpr.Method.Name != "Push" && callExpr.Method.Name != "AddToSet")
            {
                throw new NotSupportedException(
                    String.Format("{0} is not supported.", callExpr.Method.Name));
            }

            return callExpr.Method;
        }

        private static object GetSupprotedArgument(MethodCallExpression expr)
        {
            var constantExprIndex = expr.Object == null ? 1 : 0;
            var value = expr.Arguments[constantExprIndex].Eval();

            switch (expr.Method.Name)
            {
                case "Push":
                case "AddToSet":
                    return value is IEnumerable<object> ? value : ((IEnumerable)value).Cast<object>().ToList();
                default:
                    throw new NotSupportedException();
            }
        }

        public MethodCallUpdate(PropertyInfo property, MethodInfo method, object argument)
        {
            Debug.Assert(property != null, "property should not be null");
            Debug.Assert(method != null, "method should not be null");

            this.Property = property;
            this.Method = method;
            this.Argument = argument;
        }

        public PropertyInfo Property { get; private set; }
        public MethodInfo Method { get; private set; }
        public object Argument { get; private set; }

        public void Fill(IPropertyUpdateOperator optr, BsonDocument doc)
        {
            switch (this.Method.Name)
            {
                case "Push":
                    optr.PutPushUpdate(doc, (IEnumerable<object>)this.Argument);
                    break;
                case "AddToSet":
                    optr.PutAddToSetUpdate(doc, (IEnumerable<object>)this.Argument);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

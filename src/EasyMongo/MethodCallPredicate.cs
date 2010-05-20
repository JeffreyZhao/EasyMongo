using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;
using EasyMongo.Expressions;
using System.Collections;

namespace EasyMongo
{
    internal class MethodCallPredicate : IPropertyPredicate
    {
        public MethodCallPredicate(MethodCallExpression expr)
        {
            this.Method = CheckSupportedMethod(expr.Method);
            this.Property = GetProperty(expr);
            this.Constant = GetConstant(this.Property.PropertyType, expr);
        }

        private static PropertyInfo GetProperty(MethodCallExpression expr)
        {
            var maybePropExpr = expr.Object ?? expr.Arguments[0];

            var memberExpr = maybePropExpr as MemberExpression;
            if (memberExpr == null) throw new ArgumentException(maybePropExpr + " is not a property.");

            var property = memberExpr.Member as PropertyInfo;
            if (property == null) throw new ArgumentException(maybePropExpr + " is not a property.");

            return property;
        }

        private static object GetConstant(Type propertyType, MethodCallExpression expr)
        {
            var constantExprIndex = expr.Object == null ? 1 : 0;
            var value = expr.Arguments[constantExprIndex].Eval();

            if (expr.Method.Name == "Contains")
            {
                if (!propertyType.IsEnum) return value;
                if (value is Enum) return value;

                var name = Enum.GetName(propertyType, value);
                return Enum.Parse(propertyType, name);
            }
            else if (expr.Method.Name == "ContainedIn")
            {
                return ((IEnumerable)value).Cast<object>();
            }
            else
            {
                throw new NotSupportedException("Only support Contains or ContainedIn method.");
            }
        }

        private static MethodInfo CheckSupportedMethod(MethodInfo method)
        {
            if (method.Name != "Contains" && method.Name != "ContainedIn")
            {
                throw new NotSupportedException("Only support Contains or ContainedIn method.");
            }

            return method;
        }

        public MethodInfo Method { get; private set; }
        public PropertyInfo Property { get; private set; }
        public object Constant { get; private set; }

        public void Fill(IPropertyPredicateOperator optr, Document doc)
        {
            switch (this.Method.Name)
            { 
                case "Contains":
                    optr.PutContainsPredicate(doc, this.Constant);
                    break;
                case "ContainedIn":
                    optr.PutContainedInPredicate(doc, (IEnumerable<object>)this.Constant);
                    break;
            }
        }
    }
}

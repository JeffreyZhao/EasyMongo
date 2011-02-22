using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;
using EasyMongo.Expressions;
using System.Collections;
using MongoDB.Bson;

namespace EasyMongo
{
    internal class MethodCallPredicate : IPropertyPredicate
    {
        public MethodCallPredicate(MethodCallExpression expr)
        {
            this.Method = CheckSupportedMethod(expr.Method);
            this.Property = GetProperty(expr);
            this.Constants = GetConstants(this.Property.PropertyType, expr);
        }

        private static PropertyInfo GetProperty(MethodCallExpression expr)
        {
            var maybePropExpr = expr.Object ?? expr.Arguments[0];

            if (maybePropExpr.NodeType == ExpressionType.Convert)
            {
                maybePropExpr = ((UnaryExpression)maybePropExpr).Operand;
            }

            var memberExpr = maybePropExpr as MemberExpression;
            if (memberExpr == null) throw new ArgumentException(maybePropExpr + " is not a property.");

            var property = memberExpr.Member as PropertyInfo;
            if (property == null) throw new ArgumentException(maybePropExpr + " is not a property.");

            return property;
        }

        private static object[] GetConstants(Type propertyType, MethodCallExpression expr)
        {
            var constantExprIndex = expr.Object == null ? 1 : 0;
            var value = expr.Arguments[constantExprIndex].Eval();

            if (expr.Method.Name == "Contains")
            {
                return new[] { value };
            }
            else if (expr.Method.Name == "In")
            {
                return new[] { ((IEnumerable)value).Cast<object>() };
            }
            else if (expr.Method.Name == "Matches")
            {
                return new[] { value, expr.Arguments[constantExprIndex + 1].Eval() };
            }
            else
            {
                throw new NotSupportedException(
                    String.Format("{0} is not supported.", expr.Method.Name));
            }
        }

        private static MethodInfo CheckSupportedMethod(MethodInfo method)
        {
            if (method.Name != "Contains" && method.Name != "In" && method.Name != "Matches")
            {
                throw new NotSupportedException(
                    String.Format("{0} is not supported.", method.Name));
            }

            return method;
        }

        public MethodInfo Method { get; private set; }
        public PropertyInfo Property { get; private set; }
        public object[] Constants { get; private set; }

        public void Fill(IPropertyPredicateOperator optr, QueryDocument doc)
        {
            switch (this.Method.Name)
            {
                case "Contains":
                    optr.PutContainsPredicate(doc, this.Constants[0]);
                    break;
                case "In":
                    optr.PutInPredicate(doc, (IEnumerable<object>)this.Constants[0]);
                    break;
                case "Matches":
                    optr.PutRegexMatchPredicate(doc, (string)this.Constants[0], (string)this.Constants[1]);
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;
using EasyMongo.Expressions;

namespace EasyMongo
{
    internal class MethodCallPredicate : IPropertyPredicate
    {
        public MethodCallPredicate(MethodCallExpression expr)
        {
            this.Method = CheckSupportedMethod(expr.Method);
            this.Property = GetProperty(expr);
            this.Constant = GetConstant(expr);
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

        private static object GetConstant(MethodCallExpression expr)
        {
            var constantExprIndex = expr.Object == null ? 1 : 0;
            return expr.Arguments[constantExprIndex].Eval();
        }

        private static MethodInfo CheckSupportedMethod(MethodInfo method)
        {
            if (method.Name != "Contains")
            {
                throw new NotSupportedException("Only support Contains method.");
            }

            return method;
        }

        public MethodInfo Method { get; private set; }
        public PropertyInfo Property { get; private set; }
        public object Constant { get; private set; }

        public void Fill(PropertyMapper mapper, Document doc)
        {
            switch (this.Method.Name)
            { 
                case "Contains":
                    mapper.PutContainsPredicate(doc, this.Constant);
                    break;
            }
        }
    }
}

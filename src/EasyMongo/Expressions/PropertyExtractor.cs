using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyMongo.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyMongo.Expressions
{
    internal class PropertyExtractor : ExpressionVisitor
    {
        private ParameterExpression m_valuesExpr;
        private HashSet<PropertyInfo> m_properties;

        public HashSet<PropertyInfo> Extract<TResult>(Expression expr, out Func<Dictionary<PropertyInfo, object>, TResult> generator)
        {
            this.m_valuesExpr = Expression.Parameter(typeof(Dictionary<PropertyInfo, object>), "values");
            this.m_properties = new HashSet<PropertyInfo>();

            var extractedExpr = this.Visit(expr);
            var lambdaExpr = Expression.Lambda<Func<Dictionary<PropertyInfo, object>, TResult>>(extractedExpr, this.m_valuesExpr);
            generator = lambdaExpr.Compile();

            return this.m_properties;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            // n.Name
            // ->
            // (string)values[propertyInfo]

            // var propertyInfo = m.Member
            var propertyInfo = m.Member as PropertyInfo;
            var paramExpr = m.Expression as ParameterExpression;

            if (propertyInfo == null || paramExpr == null)
                return base.VisitMemberAccess(m);

            var indexProperty = typeof(Dictionary<PropertyInfo, object>).GetProperty("Item");
            var propertyExpr = Expression.Constant(propertyInfo);
            var indexExpr = Expression.Call(this.m_valuesExpr, indexProperty.GetGetMethod(), propertyExpr);
            var castExpr = Expression.Convert(indexExpr, propertyInfo.PropertyType);

            this.m_properties.Add(propertyInfo);
            return castExpr;
        }
    }
}

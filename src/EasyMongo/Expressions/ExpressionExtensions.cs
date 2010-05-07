using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace EasyMongo.Expressions
{
    public static class ExpressionExtensions
    {
        public static object Eval(this Expression expr)
        {
            var constantExpr = expr as ConstantExpression;
            if (constantExpr != null) return constantExpr.Value;

            var lambdaExpr =
                Expression.Lambda<Func<object>>(
                    Expression.Convert(expr, typeof(object)));
            var func = lambdaExpr.Compile();
            return func();
        }
    }
}

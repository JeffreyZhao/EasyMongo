using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;
using EasyMongo.Expressions;

namespace EasyMongo
{
    internal class BinaryPredicate : IPropertyPredicate
    {
        public BinaryPredicate(BinaryExpression expr)
        {
            this.Property = GetProperty(expr.Left);
            this.Constant = GetConstant(this.Property.PropertyType, expr.Right);

            if (this.Property.PropertyType.IsEnum && !(this.Constant is Enum))
            {
                this.Constant = Enum.GetName(this.Property.PropertyType, this.Constant);
            }

            this.OpType = GetSupportedOpType(expr.NodeType);
        }

        private static object GetConstant(Type propertyType, Expression expr)
        {
            var value = expr.Eval();
            if (!propertyType.IsEnum) return value;
            if (value is Enum) return value;

            var name = Enum.GetName(propertyType, value);
            return Enum.Parse(propertyType, name);
        }

        private static PropertyInfo GetProperty(Expression expr)
        {
            // enum comparison need to convert first
            if (expr.NodeType == ExpressionType.Convert)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            var memberExpr = expr as MemberExpression;
            if (memberExpr == null) throw new ArgumentException(expr + " is not a property.");

            var property = memberExpr.Member as PropertyInfo;
            if (property == null) throw new ArgumentException(expr + " is not a property.");

            return property;
        }

        private static ExpressionType GetSupportedOpType(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.LessThan:
                    return type;
                default:
                    throw new NotSupportedException(type + "is not supported");
            }
        }

        public PropertyInfo Property { get; private set; }
        public object Constant { get; private set; }
        public ExpressionType OpType { get; private set; }
        
        public void Fill(PropertyMapper mapper, Document doc)
        {
            switch (this.OpType)
            {
                case ExpressionType.Equal:
                    mapper.PutEqualPredicate(doc, this.Constant);
                    break;
                case ExpressionType.GreaterThan:
                    mapper.PutGreaterThanPredicate(doc, this.Constant);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    mapper.PutGreaterThanOrEqualPredicate(doc, this.Constant);
                    break;
                case ExpressionType.LessThan:
                    mapper.PutLessThanPredicate(doc, this.Constant);
                    break;
                case ExpressionType.LessThanOrEqual:
                    mapper.PutLessThanOrEqualPredicate(doc, this.Constant);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}

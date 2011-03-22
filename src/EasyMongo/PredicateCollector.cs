using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using EasyMongo.Expressions;

namespace EasyMongo
{
    internal class PredicateCollector : ExpressionVisitor
    {
        private List<IPropertyPredicate> m_predicates;

        public List<IPropertyPredicate> Collect(Expression predicate)
        {
            this.m_predicates = new List<IPropertyPredicate>();
            this.Visit(predicate);
            return this.m_predicates;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                case "In":
                case "Matches":
                    this.m_predicates.Add(new MethodCallPredicate(m));
                    break;
                default:
                    throw new NotSupportedException(
                        String.Format("{0} is not supported", m.Method));
            }

            return m;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                    Visit(b.Left);
                    Visit(b.Right);
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.LessThan:
                    var predicate = new BinaryPredicate(b);
                    this.m_predicates.Add(predicate);
                    break;
                default:
                    throw new NotSupportedException(
                        String.Format("{0} is not supported", b.NodeType));
            }

            return b;
        }
    }
}

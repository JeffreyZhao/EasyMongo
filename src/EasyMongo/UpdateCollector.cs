using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyMongo.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyMongo
{
    internal class UpdateCollector : ExpressionVisitor
    {
        public List<IPropertyUpdate> Collect(Expression expr)
        {
            expr = new PartialEvaluator().Eval(expr);

            this.m_updates = new List<IPropertyUpdate>();
            this.Visit(expr);
            return this.m_updates;
        }

        private List<IPropertyUpdate> m_updates;

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var property = assignment.Member as PropertyInfo;
            if (property == null)
            {
                throw new NotSupportedException(
                    String.Format("{0} is not a property.", assignment.Member));
            }

            IPropertyUpdate update;
            var expr = assignment.Expression;
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                    update = ConstantUpdate.Create(property, (ConstantExpression)expr);
                    break;
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    update = BinaryUpdate.Create(property, (BinaryExpression)expr);
                    break;
                case ExpressionType.Call:
                    update = MethodCallUpdate.Create(property, (MethodCallExpression)expr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            this.m_updates.Add(update);

            return assignment;
        }
    }
}

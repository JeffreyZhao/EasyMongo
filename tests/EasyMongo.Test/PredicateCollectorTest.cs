using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Xunit;

namespace EasyMongo.Test
{
    public class PredicateCollectorTest
    {
        public class User
        {
            public int UserID { get; set; }
            public int Age { get; set; }
            public int Size { get; set; }
        }

        [Fact]
        public void SingleEqualPredicate()
        {
            Expression<Func<User, bool>> expr = u => u.UserID == 10;
            var predicate = new PredicateCollector().Collect(expr.Body).Single() as BinaryPredicate;

            Assert.NotNull(predicate);
            Assert.Equal(typeof(User).GetProperty("UserID"), predicate.Property);
            Assert.Equal(ExpressionType.Equal, predicate.OpType);
            Assert.Equal(10, predicate.Constant);
        }

        [Fact]
        public void SingleLessThanOrEqualPredicateWithClosure()
        {
            int maxAge = 100;
            Expression<Func<User, bool>> expr = u => u.Age <= maxAge;
            var predicate = new PredicateCollector().Collect(expr.Body).Single() as BinaryPredicate;

            Assert.NotNull(predicate);
            Assert.Equal(typeof(User).GetProperty("Age"), predicate.Property);
            Assert.Equal(ExpressionType.LessThanOrEqual, predicate.OpType);
            Assert.Equal(maxAge, predicate.Constant);
        }

        [Fact]
        public void LessThanAndLargerThanWithClosure()
        {
            int maxAge = 100;
            Expression<Func<User, bool>> expr = u => u.Age < maxAge && u.Size == 10;
            var predicates = new PredicateCollector().Collect(expr.Body)
                .Cast<BinaryPredicate>().OrderBy(bp => bp.Property.Name).ToList();

            Assert.Equal(2, predicates.Count);

            var agePredicate = predicates[0];
            Assert.Equal(typeof(User).GetProperty("Age"), agePredicate.Property);
            Assert.Equal(ExpressionType.LessThan, agePredicate.OpType);
            Assert.Equal(maxAge, agePredicate.Constant);

            var sizePredicate = predicates[1];
            Assert.Equal(typeof(User).GetProperty("Size"), sizePredicate.Property);
            Assert.Equal(ExpressionType.Equal, sizePredicate.OpType);
            Assert.Equal(10, sizePredicate.Constant);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Xunit;

namespace EasyMongo.Test
{
    public class MethodCallPredicateTest
    {
        public class Article
        {
            public string Type { get; set; }
        }

        [Fact]
        public void CreateContainedIn()
        {
            Expression<Func<Article, bool>> expr = a => a.Type.ContainedIn(new[] { "Hello", "World" });
            var callPredicate = new MethodCallPredicate((MethodCallExpression)expr.Body);

            Assert.Equal("ContainedIn", callPredicate.Method.Name);
            Assert.Equal(typeof(Article).GetProperty("Type"), callPredicate.Property);

            var array = ((IEnumerable<object>)callPredicate.Constant).ToArray();
            Assert.Equal("Hello", array[0]);
            Assert.Equal("World", array[1]);
        }
    }
}

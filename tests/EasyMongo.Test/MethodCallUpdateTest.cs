using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;
using Moq;

namespace EasyMongo.Test
{
    public class MethodCallUpdateTest
    {
        public class Article
        {
            public List<string> Tags { get; set; }
        }

        [Fact]
        public void CreatePush()
        {
            var property = typeof(Article).GetProperty("Tags");
            Expression<Func<Article, List<string>>> expr = a => a.Tags.Push("hello", "world");

            var update = MethodCallUpdate.Create(property, (MethodCallExpression)expr.Body);
            Assert.Equal(property, update.Property);
            Assert.Equal("Push", update.Method.Name);

            var array = ((IEnumerable<object>)update.Argument).ToArray();
            Assert.Equal("hello", array[0]);
            Assert.Equal("world", array[1]);
        }

        [Fact]
        public void FillPush()
        {
            var property = typeof(Article).GetProperty("Tags");
            var method = typeof(UpdateExtensions).GetMethod("Push", BindingFlags.Static | BindingFlags.Public);
            var argument = new[] { "hello", "world" };
            var update = new MethodCallUpdate(property, method, argument);
            var doc = new Document();

            var mockOperator = new Mock<IPropertyUpdateOperator>();
            mockOperator.Setup(o => o.PutPushUpdate(doc, argument)).Verifiable();
            update.Fill(mockOperator.Object, doc);

            mockOperator.Verify();
        }
    }
}

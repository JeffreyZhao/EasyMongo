using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Xunit;
using Moq;
using MongoDB.Driver;

namespace EasyMongo.Test
{
    public class BinaryUpdateTest
    {
        public class User
        {
            public int UserID { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void CreateAdd()
        {
            var property = typeof(User).GetProperty("UserID");
            Expression<Func<User, int>> expr = u => u.UserID + 10;

            var update = BinaryUpdate.Create(property, (BinaryExpression)expr.Body);
            Assert.Equal(property, update.Property);
            Assert.Equal(ExpressionType.Add, update.OpType);
            Assert.Equal(10, update.Constant);
        }

        [Fact]
        public void FillAdd()
        { 
            var property = typeof(User).GetProperty("UserID");
            var update = new BinaryUpdate(property, ExpressionType.Add, 10);
            var doc = new Document();
            
            var mockOperator = new Mock<IPropertyUpdateOperator>();
            mockOperator.Setup(o => o.PutAddUpdate(doc, 10)).Verifiable();
            update.Fill(mockOperator.Object, doc);

            mockOperator.Verify();
        }

        [Fact]
        public void CreateSubtract()
        {
            var property = typeof(User).GetProperty("Age");
            Expression<Func<User, int>> expr = u => u.Age - 5;

            var update = BinaryUpdate.Create(property, (BinaryExpression)expr.Body);
            Assert.Equal(property, update.Property);
            Assert.Equal(ExpressionType.Subtract, update.OpType);
            Assert.Equal(5, update.Constant);
        }

        [Fact]
        public void FillSubtract()
        {
            var property = typeof(User).GetProperty("Age");
            var update = new BinaryUpdate(property, ExpressionType.Subtract, 5);
            var doc = new Document();

            var mockOperator = new Mock<IPropertyUpdateOperator>();
            mockOperator.Setup(o => o.PutSubtractUpdate(doc, 5)).Verifiable();
            update.Fill(mockOperator.Object, doc);

            mockOperator.Verify();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Linq.Expressions;

namespace EasyMongo.Test
{
    public class UpdateCollectorTest
    {
        public class User
        {
            public int UserID { get; set; }
            public int Age { get; set; }
            public int Size { get; set; }
            public List<string> Tags { get; set; }
        }

        [Fact]
        public void Mixed()
        {
            Expression<Func<User, User>> expr = u => new User
            {
                UserID = 10,
                Age = u.Age + 3,
                Size = u.Age - 1,
                Tags = u.Tags.Push("AAA", "BBB", "CCC")
            };

            var updates = new UpdateCollector().Collect(expr.Body).ToList();

            Assert.Equal(4, updates.Count);

            var userIdUpdate = (ConstantUpdate)updates[0];
            Assert.Equal(typeof(User).GetProperty("UserID"), userIdUpdate.Property);
            Assert.Equal(10, userIdUpdate.Value);

            var ageUpdate = (BinaryUpdate)updates[1];
            Assert.Equal(typeof(User).GetProperty("Age"), ageUpdate.Property);
            Assert.Equal(ExpressionType.Add, ageUpdate.OpType);
            Assert.Equal(3, ageUpdate.Constant);

            var sizeUpdate = (BinaryUpdate)updates[2];
            Assert.Equal(typeof(User).GetProperty("Size"), sizeUpdate.Property);
            Assert.Equal(ExpressionType.Subtract, sizeUpdate.OpType);
            Assert.Equal(1, sizeUpdate.Constant);

            var pushUpdate = (MethodCallUpdate)updates[3];
            Assert.Equal(typeof(User).GetProperty("Tags"), pushUpdate.Property);
            Assert.Equal("Push", pushUpdate.Method.Name);
            var array = ((IEnumerable<object>)pushUpdate.Argument).ToArray();
            Assert.Equal(3, array.Length);
            Assert.Equal("AAA", array[0]);
            Assert.Equal("BBB", array[1]);
            Assert.Equal("CCC", array[2]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using MongoDB.Driver;
using Xunit;
using System.Reflection;

namespace EasyMongo.Test
{
    public class PropertyMapperTest
    {
        [Fact]
        public void PutEqualPredicate()
        {
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Name).Returns("Hello");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var doc = new Document();
            mapper.PutEqualPredicate(doc, 20);

            Assert.Equal(1, doc.Count);
            Assert.Equal(20, doc["Hello"]);
        }

        [Fact]
        public void PutGreatThanAndLessThanPredicate()
        {
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Name).Returns("Hello");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var doc = new Document();
            mapper.PutGreaterThanPredicate(doc, 10);
            mapper.PutLessThanPredicate(doc, 20);

            Assert.Equal(1, doc.Count);

            var innerDoc = doc["Hello"] as Document;
            Assert.Equal(10, innerDoc["$gt"]);
            Assert.Equal(20, innerDoc["$lt"]);
        }

        public void PutGreatThanAndEqualsPredicate()
        {
            var mockAgeDescriptor = new Mock<IPropertyDescriptor>();
            mockAgeDescriptor.Setup(d => d.Name).Returns("Age");
        }

        [Flags]
        public enum UserTypes
        { 
            Type1 = 1,
            Type2 = 2,
            Type3 = 4
        }

        public enum Gender
        { 
            Male,
            Female
        }

        public class User
        {
            public int UserID { get; set; }
            public Gender Gender { get; set; }
            public UserTypes Types { get; set; }
            public List<string> Hobbies { get; set; }
        }

        [Fact]
        public void SetArrayValue()
        {
            var property = typeof(User).GetProperty("Hobbies");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Hobbies");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var user = new User();

            var doc = new Document().Append("Hobbies", new[] { "Ball", "Piano" });
            mapper.SetEntityValue(user, doc);

            Assert.Equal(2, user.Hobbies.Count);
            Assert.Equal("Ball", user.Hobbies[0]);
            Assert.Equal("Piano", user.Hobbies[1]);
        }

        [Fact]
        public void SetEnumValue()
        {
            var property = typeof(User).GetProperty("Gender");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Gender");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var user = new User();

            var doc = new Document().Append("Gender", "Female");
            mapper.SetEntityValue(user, doc);
            Assert.Equal(Gender.Female, user.Gender);
        }

        [Fact]
        public void SetFlagsValue()
        {
            var property = typeof(User).GetProperty("Types");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Types");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var user = new User();

            var doc = new Document().Append("Types", new[] { "Type1", "Type3" });
            mapper.SetEntityValue(user, doc);
            Assert.Equal(UserTypes.Type1 | UserTypes.Type3, user.Types);
        }

        [Fact]
        public void PutArrayState()
        {
            var property = typeof(User).GetProperty("Hobbies");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Hobbies");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var user = new User
            {
                Hobbies = new List<string> { "Ball", "Piano" }
            };

            var state = new Dictionary<PropertyInfo, object>();
            mapper.PutEntityState(state, user);

            var list = (List<object>)state[property];
            Assert.Equal(2, list.Count);
            Assert.Equal("Ball", list[0]);
            Assert.Equal("Piano", list[1]);
        }

        /*[Fact]
        public void SetArrayState()
        {
            var property = typeof(User).GetProperty("Hobbies");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Hobbies");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var state = new Dictionary<PropertyInfo, object>()
            {
                { property, new List<object> { "Ball", "Piano" } }
            };

            var user = new User();
            mapper.SetEntityState(user, state);

            Assert.Equal(2, user.Hobbies.Count);
            Assert.Equal("Ball", user.Hobbies[0]);
            Assert.Equal("Piano", user.Hobbies[1]);
        }*/
    }
}

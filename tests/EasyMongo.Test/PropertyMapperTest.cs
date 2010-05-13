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
            mapper.SetValue(user, doc);

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
            mapper.SetValue(user, doc);
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
            mapper.SetValue(user, doc);
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

            var state = new Dictionary<PropertyMapper, object>();
            mapper.PutState(state, user);

            var arrayState = (ArrayState)state[mapper];
            Assert.Same(user.Hobbies, arrayState.Container);
            Assert.Equal(2, arrayState.Items.Count);
            Assert.Equal("Ball", arrayState.Items[0]);
            Assert.Equal("Piano", arrayState.Items[1]);
        }

        [Fact]
        public void TryPutStateChange_NewArray()
        {
            var property = typeof(User).GetProperty("Hobbies");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Hobbies");

            var mapper = new PropertyMapper(mockDescriptor.Object);

            var originalState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, new ArrayState(new List<string> { "Good", "Girl" }) }
            };
            var currentState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, new ArrayState(new List<string> { "Bad", "Boy" }) }
            };

            var doc = new Document();
            mapper.TryPutStateChange(doc, originalState, currentState);

            var innerDoc = (Document)doc["$set"];
            Assert.Equal(1, innerDoc.Count);

            var array = (object[])innerDoc["Hobbies"];
            Assert.Equal("Bad", array[0]);
            Assert.Equal("Boy", array[1]);
        }

        [Fact]
        public void TryPutStateChange_AddItemToArray()
        {
            var property = typeof(User).GetProperty("Hobbies");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Hobbies");

            var mapper = new PropertyMapper(mockDescriptor.Object);

            var list = new List<string> { "Good", "Girl" };
            var originalState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, new ArrayState(list) }
            };

            list.AddRange(new[] { "Hello", "World" });
            var currentState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, new ArrayState(list) }
            };

            var doc = new Document();
            mapper.TryPutStateChange(doc, originalState, currentState);

            var innerDoc = (Document)doc["$pushAll"];
            Assert.Equal(1, innerDoc.Count);

            var array = (object[])innerDoc["Hobbies"];
            Assert.Equal("Hello", array[0]);
            Assert.Equal("World", array[1]);
        }

        [Fact]
        public void TryPutStateChange_RemoveItemFromArray()
        {
            var property = typeof(User).GetProperty("Hobbies");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Hobbies");

            var mapper = new PropertyMapper(mockDescriptor.Object);

            var list = new List<string> { "Good", "Girl", "Hello", "World" };
            var originalState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, new ArrayState(list) }
            };

            list.Remove("Good");
            list.Remove("Girl");
            var currentState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, new ArrayState(list) }
            };

            var doc = new Document();
            mapper.TryPutStateChange(doc, originalState, currentState);

            var innerDoc = (Document)doc["$pullAll"];
            Assert.Equal(1, innerDoc.Count);

            var array = (object[])innerDoc["Hobbies"];
            Assert.Equal("Good", array[0]);
            Assert.Equal("Girl", array[1]);
        }

        [Fact]
        public void TryPutStateChange_ChangeFlags()
        {
            var property = typeof(User).GetProperty("Types");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Types");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var originalState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, UserTypes.Type1 | UserTypes.Type2 }
            };
            var currentState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, UserTypes.Type2 | UserTypes.Type3 }
            };

            var doc = new Document();
            mapper.TryPutStateChange(doc, originalState, currentState);

            var innerDoc = (Document)doc["$set"];
            Assert.Equal(1, innerDoc.Count);

            var array = (string[])innerDoc["Types"];
            Assert.Equal("Type2", array[0]);
            Assert.Equal("Type3", array[1]);
        }

        [Fact]
        public void TryPutStateChange_ChangeEnum()
        {
            var property = typeof(User).GetProperty("Gender");
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Property).Returns(property);
            mockDescriptor.Setup(d => d.Name).Returns("Gender");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var originalState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, Gender.Female }
            };
            var currentState = new Dictionary<PropertyMapper, object>()
            {
                { mapper, Gender.Male }
            };

            var doc = new Document();
            mapper.TryPutStateChange(doc, originalState, currentState);

            var innerDoc = (Document)doc["$set"];
            Assert.Equal(1, innerDoc.Count);

            Assert.Equal("Male", innerDoc["Gender"]);
        }
    }
}

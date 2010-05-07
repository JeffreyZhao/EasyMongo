using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using MongoDB.Driver;
using Xunit;

namespace EasyMongo.Test
{
    public class PropertyMapperTest
    {
        [Fact]
        public void FillEqualPredicate()
        {
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Name).Returns("Hello");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var doc = new Document();
            mapper.FillEqualPredicate(doc, 20);

            Assert.Equal(1, doc.Count);
            Assert.Equal(20, doc["Hello"]);
        }

        public void FillGreatThanAndLessThanPredicate()
        {
            var mockDescriptor = new Mock<IPropertyDescriptor>();
            mockDescriptor.Setup(d => d.Name).Returns("Hello");

            var mapper = new PropertyMapper(mockDescriptor.Object);
            var doc = new Document();
            mapper.FillGreaterThanPredicate(doc, 10);
            mapper.FillLessThanPredicate(doc, 20);

            Assert.Equal(1, doc.Count);

            var innerDoc = doc["Hello"] as Document;
            Assert.Equal(10, innerDoc["$gt"]);
            Assert.Equal(20, innerDoc["$lt"]);
        }

        public void FillGreatThanAndEqualsPredicate()
        {
            var mockAgeDescriptor = new Mock<IPropertyDescriptor>();
            mockAgeDescriptor.Setup(d => d.Name).Returns("Age");
        }
    }
}

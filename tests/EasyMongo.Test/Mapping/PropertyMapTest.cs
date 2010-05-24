using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using EasyMongo.Mapping;

namespace EasyMongo.Test.Mapping
{
    public class PropertyMapTest
    {
        public class TestEntity
        {
            public int TestProperty { get; set; }
            public string ChangeWithProperty { get; set; }
            public bool AnotherProperty { get; set; }
        }

        [Fact]
        public void PropertyOnly()
        {
            IPropertyMap map = new PropertyMap<TestEntity, int>(e => e.TestProperty);
            var descriptor = map.ToDescriptor();
            
            Assert.Equal(typeof(TestEntity).GetProperty("TestProperty"), descriptor.Property);
            Assert.Equal("TestProperty", descriptor.Name);
            Assert.Equal(false, descriptor.IsIdentity);
            Assert.Equal(false, descriptor.HasDefaultValue);
        }

        [Fact]
        public void Identity()
        {
            IPropertyMap map = new PropertyMap<TestEntity, int>(e => e.TestProperty).Identity();
            var descriptor = map.ToDescriptor();

            Assert.Equal(typeof(TestEntity).GetProperty("TestProperty"), descriptor.Property);
            Assert.Equal("TestProperty", descriptor.Name);
            Assert.Equal(true, descriptor.IsIdentity);
            Assert.Equal(false, descriptor.HasDefaultValue);
        }
        
        [Fact]
        public void DefaultValue()
        {
            IPropertyMap map = new PropertyMap<TestEntity, int>(e => e.TestProperty).DefaultValue(20);
            var descriptor = map.ToDescriptor();

            Assert.Equal(typeof(TestEntity).GetProperty("TestProperty"), descriptor.Property);
            Assert.Equal("TestProperty", descriptor.Name);
            Assert.Equal(false, descriptor.IsIdentity);
            Assert.Equal(true, descriptor.HasDefaultValue);
            Assert.Equal(20, descriptor.GetDefaultValue());
        }

        [Fact]
        public void PropertyName()
        {
            IPropertyMap map = new PropertyMap<TestEntity, int>(e => e.TestProperty).Name("Another");
            var descriptor = map.ToDescriptor();

            Assert.Equal(typeof(TestEntity).GetProperty("TestProperty"), descriptor.Property);
            Assert.Equal("Another", descriptor.Name);
            Assert.Equal(false, descriptor.IsIdentity);
            Assert.Equal(false, descriptor.HasDefaultValue);
            Assert.Equal(0, descriptor.ChangeWithProperties.Count);
        }

        [Fact]
        public void ChangeWithProperties()
        {
            IPropertyMap map = new PropertyMap<TestEntity, int>(e => e.TestProperty)
                .ChangeWith(e => e.ChangeWithProperty).ChangeWith(e => e.AnotherProperty);

            var descriptor = map.ToDescriptor();

            Assert.Equal(typeof(TestEntity).GetProperty("TestProperty"), descriptor.Property);
            Assert.Equal("TestProperty", descriptor.Name);
            Assert.Equal(false, descriptor.IsIdentity);
            Assert.Equal(false, descriptor.HasDefaultValue);

            Assert.Equal(2, descriptor.ChangeWithProperties.Count);
            Assert.True(descriptor.ChangeWithProperties.Contains(typeof(TestEntity).GetProperty("ChangeWithProperty")));
            Assert.True(descriptor.ChangeWithProperties.Contains(typeof(TestEntity).GetProperty("AnotherProperty")));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using MongoDB.Bson;

namespace EasyMongo.Types
{
    internal class EnumProcessor : BasicProcessor, IArrayProcessor
    {
        public EnumProcessor(PropertyInfo property)
        {
            this.Property = property;
        }

        public PropertyInfo Property { get; private set; }

        public override BsonValue ToBsonValue(object value)
        {
            if (this.Property.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                return new BsonArray((IEnumerable<string>)value.ToString().Split(new[] { ", " }, StringSplitOptions.None));
            }
            else
            {
                return new BsonString(value.ToString());
            }
        }

        public override object FromBsonValue(BsonValue bsonValue)
        {
            if (this.Property.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                var itemNames = ((BsonArray)bsonValue).Select(v => v.AsString).ToArray();
                return Enum.Parse(this.Property.PropertyType, String.Join(",", itemNames));
            }
            else
            {
                return Enum.Parse(this.Property.PropertyType, bsonValue.AsString);
            }
        }

        public BsonArray GetPushingValues(object originalState, object currentState)
        {
            return null;
        }

        public BsonArray GetValues(IEnumerable<object> items)
        {
            throw new NotSupportedException();
        }

        public BsonValue GetContainingValue(object value)
        {
            if (this.Property.PropertyType.IsDefined(typeof(FlagsAttribute), false))
            {
                var itemNames = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
                if (itemNames.Length != 1)
                {
                    throw new NotSupportedException("Contains predicate could be used with one and only one item.");
                }

                return new BsonString(itemNames[0]);
            }
            else
            {
                throw new NotSupportedException("Only support Flags");
            }
        }
    }
}

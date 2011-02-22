using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using MongoDB.Driver;
using MongoDB.Bson;

namespace EasyMongo.Types
{
    internal class ArrayProcessor : IArrayProcessor
    {
        public ArrayProcessor(PropertyInfo property)
        {
            this.Property = property;
        }

        public PropertyInfo Property { get; private set; }

        public BsonValue ToBsonValue(object value)
        {
            if (value == null) return null;
            return new BsonArray(((IEnumerable)value).Cast<object>());
        }

        public object FromBsonValue(BsonValue bsonValue)
        {
            if (bsonValue.IsBsonNull) return null;

            var array = (IList)Activator.CreateInstance(this.Property.PropertyType);
            var bsonArray = bsonValue as BsonArray;

            if (bsonArray != null)
            {
                foreach (var item in bsonArray.RawValues) array.Add(item);
            }

            return array;
        }

        public object ToStateValue(object value)
        {
            return value == null ? null : new ArrayState((IList)value);
        }

        public bool IsStateChanged(object originalState, object currentState)
        {
            if (currentState == null && originalState == null) return false;
            if (currentState == null && originalState != null) return true;
            if (currentState != null && originalState == null) return true;

            var currentArray = (ArrayState)currentState;
            var originalArray = (ArrayState)originalState;

            if (!Object.ReferenceEquals(currentArray.Container, originalArray.Container)) return true;

            if (originalArray.Items.Count > currentArray.Items.Count)
            {
                throw new NotSupportedException("Does not support item removal in array.");
            }

            for (int i = 0; i < originalArray.Items.Count; i++)
            {
                if (!Object.Equals(currentArray.Items[i], originalArray.Items[i]))
                {
                    throw new NotSupportedException("Does not support item removal in array.");
                }
            }

            return originalArray.Items.Count != currentArray.Items.Count;
        }

        public BsonArray GetPushingValues(object originalState, object currentState)
        {
            var originalArray = (ArrayState)originalState;
            var currentArray = (ArrayState)currentState;

            if (!Object.ReferenceEquals(originalArray.Container, currentArray.Container)) return null;

            return new BsonArray(currentArray.Items.Skip(originalArray.Items.Count).ToArray());
        }

        public BsonArray GetValues(IEnumerable<object> items)
        {
            return new BsonArray(items);
        }

        public BsonValue GetContainingValue(object value)
        {
            return BsonValue.Create(value);
        }

        public object FromStateValue(object stateValue)
        {
            if (stateValue == null) return null;

            var array = (IList)Activator.CreateInstance(this.Property.PropertyType);
            var arrayState = (ArrayState)stateValue;
            foreach (var item in arrayState.Items) array.Add(item);

            return array;
        }
    }
}

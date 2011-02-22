using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyMongo.Types;
using MongoDB.Bson;

namespace SimpleUsages.Mapping
{
    internal class LocalTimeProcessor : ITypeProcessor
    {
        public object FromStateValue(object stateValue)
        {
            return stateValue;
        }

        public bool IsStateChanged(object originalState, object currentState)
        {
            return !Object.Equals(originalState, currentState);
        }

        public object ToStateValue(object value)
        {
            return value;
        }

        public BsonValue ToBsonValue(object value)
        {
            return new BsonDateTime((DateTime)value);
        }

        public object FromBsonValue(BsonValue bsonValue)
        {
            return ((DateTime)bsonValue).ToLocalTime();
        }
    }
}

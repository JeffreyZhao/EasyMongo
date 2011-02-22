using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MongoDB.Bson;

namespace EasyMongo.Types
{
    internal class BasicProcessor : ITypeProcessor
    {
        public virtual BsonValue ToBsonValue(object value)
        {
            return BsonValue.Create(value);
        }

        public virtual object FromBsonValue(BsonValue bsonValue)
        {
            return bsonValue.RawValue;
        }

        public virtual object ToStateValue(object value)
        {
            return value;
        }

        public bool IsStateChanged(object originalState, object currentState)
        {
            return !Object.Equals(originalState, currentState);
        }

        public object FromStateValue(object stateValue)
        {
            return stateValue;
        }
    }
}

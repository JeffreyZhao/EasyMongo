using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace EasyMongo.Types
{
    public interface ITypeProcessor
    {
        BsonValue ToBsonValue(object value);

        object FromBsonValue(BsonValue bsonValue);

        object ToStateValue(object value);

        object FromStateValue(object stateValue);

        bool IsStateChanged(object originalState, object currentState);
    }
}

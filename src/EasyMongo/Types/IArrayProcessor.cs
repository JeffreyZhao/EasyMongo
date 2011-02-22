using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace EasyMongo.Types
{
    internal interface IArrayProcessor : ITypeProcessor
    {
        BsonArray GetPushingValues(object originalState, object currentState);

        BsonArray GetValues(IEnumerable<object> items);

        BsonValue GetContainingValue(object value);
    }
}

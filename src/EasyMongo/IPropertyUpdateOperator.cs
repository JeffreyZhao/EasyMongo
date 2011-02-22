using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace EasyMongo
{
    internal interface IPropertyUpdateOperator
    {
        void PutConstantUpdate(BsonDocument doc, object value);

        void PutAddUpdate(BsonDocument doc, object value);

        void PutPushUpdate(BsonDocument doc, IEnumerable<object> items);

        void PutAddToSetUpdate(BsonDocument doc, IEnumerable<object> items);
    }
}

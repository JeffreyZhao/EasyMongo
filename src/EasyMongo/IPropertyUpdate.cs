using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MongoDB.Driver;
using MongoDB.Bson;

namespace EasyMongo
{
    internal interface IPropertyUpdate
    {
        PropertyInfo Property { get; }

        void Fill(IPropertyUpdateOperator optr, BsonDocument doc);
    }
}

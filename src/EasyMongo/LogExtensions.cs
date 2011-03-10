using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EasyMongo
{
    internal static class LogExtensions
    {
        public static void WriteQuery(this TextWriter writer, MongoCollection collection, BsonDocument query, BsonDocument fields, BsonDocument sort, BsonDocument hints, int skip, int? limits)
        {
            if (writer == null) return;

            var cmd = String.Format(
                "db.{0}.find({1}, {2})",
                collection.Name,
                (query ?? new BsonDocument()).ToJson(),
                (fields ?? new BsonDocument()).ToJson());

            if (skip > 0) cmd += (".skip(" + skip + ")");
            if (limits.HasValue) cmd += (".limit(" + limits + ")");

            writer.WriteLine(cmd);
        }

        public static void WriteUpdate(this TextWriter writer, MongoCollection collection, BsonDocument query, BsonDocument update)
        {
            if (writer == null) return;

            var cmd = String.Format(
                "db.{0}.update({1}, {2}, false, true)",
                collection.Name,
                (query ?? new BsonDocument()).ToJson(),
                (update ?? new BsonDocument()).ToJson());

            writer.WriteLine(cmd);
        }
    }
}

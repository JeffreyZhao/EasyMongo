using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EasyMongo
{
    public class DataContext
    {
        private MappingSource m_mappingSource;
        private Dictionary<Type, object> m_collections;

        public DataContext(MongoDatabase database, MappingSource mappingSource)
            : this(database, mappingSource, true)
        { }

        public DataContext(MongoDatabase database, MappingSource mappingSource, bool objectTrackingEnabled)
        {
            this.m_mappingSource = mappingSource;
            this.m_collections = new Dictionary<Type, object>();

            this.ObjectTrackingEnabled = objectTrackingEnabled;
            this.Database = database;
        }

        public bool ObjectTrackingEnabled { get; private set; }
        public MongoDatabase Database { get; private set; }

        public T Get<T>(Expression<Func<T, bool>> predicate)
        {
            var mapper = this.m_mappingSource.GetEntityMapper<T>();
            var predicateDoc = mapper.GetPredicateDocument(predicate.Body);

            return default(T);
        }
    }
}

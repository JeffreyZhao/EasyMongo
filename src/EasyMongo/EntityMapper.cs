using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyMongo
{
    internal class EntityMapper
    {
        public EntityMapper(IEntityDescriptor descriptor)
        {
            this.Descriptor = descriptor;
            this.m_properties = descriptor.Properties.ToDictionary(
                p => p.Property,
                p => new PropertyMapper(p));
        }

        public Dictionary<PropertyInfo, PropertyMapper> m_properties;

        public IEntityDescriptor Descriptor { get; private set; }

        public IMongoCollection GetCollection(MongoDatabase database)
        {
            return database[this.Descriptor.CollectionName];
        }

        public Document GetPredicateDocument(Expression predicateExpr)
        {
            var propPredicates = new PredicateCollector().Collect(predicateExpr);

            var predicateDoc = new Document();
            foreach (var predicate in propPredicates)
            {
                var propertyMapper = this.m_properties[predicate.Property];
                predicate.Fill(propertyMapper, predicateDoc);
            }

            return predicateDoc;
        }
    }
}

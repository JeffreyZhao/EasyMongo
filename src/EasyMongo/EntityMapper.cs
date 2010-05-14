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

            this.m_identityMapper = this.m_properties.Values.Single(m => m.Descriptor.IsIdentity);
        }

        private Dictionary<PropertyInfo, PropertyMapper> m_properties;
        private PropertyMapper m_identityMapper;

        public IEntityDescriptor Descriptor { get; private set; }

        public IMongoCollection GetCollection(IMongoDatabase database)
        {
            return database[this.Descriptor.CollectionName];
        }

        public Document GetPredicate(Expression predicateExpr)
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

        public Document GetDocument(object entity)
        {
            var doc = new Document();
            foreach (var mapper in this.m_properties.Values)
            { 
                mapper.PutValue(doc, entity);
            }

            return doc;
        }

        public Document GetFields()
        {
            var doc = new Document();
            foreach (var mapper in this.m_properties.Values.Where(mp => !mp.IsReadOnly))
            {
                mapper.PutField(doc);
            }

            return doc;
        }

        public EntityState GetEntityState(object entity)
        {
            var state = new EntityState(this);
            foreach (var mapper in this.m_properties.Values.Where(mp => !mp.IsReadOnly))
            {
                mapper.PutState(state, entity);
            }

            return state;
        }

        public Document GetIdentity(object entity)
        { 
            var doc = new Document();
            this.m_identityMapper.PutValue(doc, entity);
            return doc;
        }

        public object GetEntity(Document doc)
        {
            var entity = Activator.CreateInstance(this.Descriptor.Type);
            foreach (var mapper in this.m_properties.Values.Where(mp => !mp.IsReadOnly))
            {
                mapper.SetValue(entity, doc);
            }

            return entity;
        }

        public Document GetStateChanged(EntityState original, EntityState current)
        {
            var result = new Document();

            foreach (var mapper in this.m_properties.Values.Where(p => !p.IsReadOnly))
            {
                if (mapper.IsChanged(original, current))
                {
                    mapper.PutStateChange(result, original, current);
                }
            }

            // TODO: dependency update

            return result;
        }
    }
}

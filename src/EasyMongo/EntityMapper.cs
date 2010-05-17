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

        public Document GetFields(Expression selector)
        {
            var doc = new Document();

            if (selector == null)
            {
                foreach (var mapper in this.m_properties.Values.Where(mp => !mp.IsReadOnly))
                {
                    mapper.PutField(doc);
                }
            }
            else
            {
                var initExpr = (MemberInitExpression)selector;
                foreach (var binding in initExpr.Bindings)
                {
                    var propInfo = (PropertyInfo)binding.Member;
                    this.m_properties[propInfo].PutField(doc);
                }
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
            var changedSet = new HashSet<PropertyInfo>();

            foreach (var mapper in this.m_properties.Values.Where(p => !p.IsReadOnly))
            {
                if (mapper.IsChanged(original, current))
                {
                    changedSet.Add(mapper.Descriptor.Property);
                    mapper.PutStateChange(result, original, current);
                }
            }

            foreach (var mapper in this.m_properties.Values.Where(p => p.IsReadOnly))
            {
                if (mapper.Descriptor.ChangeWithProperties.Any(p => changedSet.Contains(p)))
                {
                    mapper.PutStateChange(result, original, current);
                }
            }

            return result;
        }

        public  Document GetSortOrders(List<SortOrder> sortOrders)
        {
            Document result = new Document();

            foreach (var order in sortOrders)
            {
                var propExpr = (MemberExpression)order.KeySelector;
                var propInfo = (PropertyInfo)propExpr.Member;
                this.m_properties[propInfo].PutSortOrder(result, order.Descending);
            }

            return result;
        }
    }
}

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

            var identityCount = descriptor.Properties.Count(d => d.IsIdentity);
            if (identityCount <= 0)
            {
                throw new ArgumentException("No identity specified.");
            }
            else
            {
                this.m_properties = descriptor.Properties.ToDictionary(
                    d => d.Property,
                    d => new PropertyMapper(d, d.IsIdentity ? (identityCount == 1) : false));
            }

            this.m_identities = this.m_properties.Values
                .Where(m => m.Descriptor.IsIdentity)
                .ToDictionary(p => p.Descriptor.Property);
        }

        private Dictionary<PropertyInfo, PropertyMapper> m_properties;
        private Dictionary<PropertyInfo, PropertyMapper> m_identities;

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
                    mapper.PutField(doc, true);
                }

                foreach (var mapper in this.m_identities.Values)
                {
                    mapper.PutField(doc, true);
                }
            }
            else
            {
                var methodCall = (MethodCallExpression)selector;
                var include = methodCall.Method.Name == "PartialWith";

                var argsExpr = (NewArrayExpression)(methodCall).Arguments[1];
                foreach (UnaryExpression expr in argsExpr.Expressions)
                {
                    var lambdaExpr = (LambdaExpression)expr.Operand;
                    var propExpr = (MemberExpression)lambdaExpr.Body;
                    var propInfo = (PropertyInfo)propExpr.Member;

                    if (!include && this.m_identities.ContainsKey(propInfo)) continue;

                    this.m_properties[propInfo].PutField(doc, include);
                }

                if (include)
                {
                    foreach (var mapper in this.m_identities.Values)
                    {
                        mapper.PutField(doc, true);
                    }
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

            foreach (var mapper in this.m_identities.Values)
            {
                mapper.PutValue(doc, entity);
            }

            return doc;
        }

        public object GetEntity(Document doc)
        {
            var entity = Activator.CreateInstance(this.Descriptor.Type);
            foreach (var mapper in this.m_properties.Values.Where(mp => !mp.IsReadOnly))
            {
                mapper.SetValue(entity, doc);
            }

            foreach (var mapper in this.m_identities.Values)
            {
                mapper.SetValue(entity, doc);
            }

            return entity;
        }

        public Document GetStateChanged(object entity, EntityState original, EntityState current)
        {
            var result = new Document();
            var changedSet = new HashSet<PropertyInfo>();

            foreach (var mapper in this.m_properties.Values.Where(p => !p.IsReadOnly))
            {
                if (mapper.IsStateChanged(original, current))
                {
                    changedSet.Add(mapper.Descriptor.Property);
                    mapper.PutStateUpdate(result, original, current);
                }
            }

            foreach (var mapper in this.m_properties.Values.Where(p => p.IsReadOnly))
            {
                if (mapper.Descriptor.ChangeWithProperties.Any(p => changedSet.Contains(p)))
                {
                    mapper.PutValueUpdate(result, entity);
                }
            }

            return result;
        }

        public Document GetSortOrders(List<SortOrder> sortOrders)
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

        public Document GetUpdate(Expression updateExpr)
        {
            var propUpdates = new UpdateCollector().Collect(updateExpr);

            var updateDoc = new Document();
            foreach (var update in propUpdates)
            {
                var propertyMapper = this.m_properties[update.Property];
                update.Fill(propertyMapper, updateDoc);
            }

            return updateDoc;
        }
    }
}

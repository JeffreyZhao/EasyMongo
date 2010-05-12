using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Reflection;

namespace EasyMongo.Mapping
{
    public abstract class EntityMap<T> : IEntityMap
        where T : class
    {
        public EntityMap()
        {
            this.m_propertyMaps = new Dictionary<PropertyInfo, IPropertyMap>();
            this.m_collectionName = typeof(T).Name;
        }

        private Dictionary<PropertyInfo, IPropertyMap> m_propertyMaps;
        private string m_collectionName;

        public PropertyMap<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> property)
        {
            var propertyMap = new PropertyMap<T, TProperty>(property);

            if (this.m_propertyMaps.ContainsKey(propertyMap.Property))
            {
                return (PropertyMap<T, TProperty>)this.m_propertyMaps[propertyMap.Property];
            }

            this.m_propertyMaps.Add(propertyMap.Property, propertyMap);
            return propertyMap;
        }

        public void Collection(string name)
        {
            this.m_collectionName = name;
        }

        IEntityDescriptor IEntityMap.ToDescriptor()
        {
            return new EntityDescriptor
            {
                Type = typeof(T),
                CollectionName = this.m_collectionName,
                Properties = new ReadOnlyCollection<IPropertyDescriptor>(
                    this.m_propertyMaps.Select(p => p.Value.ToDescriptor()).ToList())
            };
        }
    }
}

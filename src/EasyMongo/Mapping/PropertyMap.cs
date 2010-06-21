using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using EasyMongo.Types;

namespace EasyMongo.Mapping
{
    public class PropertyMap<TEntity, TProperty> : IPropertyMap
    {
        public PropertyMap(Expression<Func<TEntity, TProperty>> property)
        {
            this.Property = (PropertyInfo)(property.Body as MemberExpression).Member;
            this.m_name = null;
            this.m_isIdentity = false;
            this.m_hasDefaultValue = false;
            this.m_defaultValueFactory = null;
            this.m_changeWithProperties = new List<PropertyInfo>();
        }

        private bool m_isIdentity;
        private string m_name;
        private bool m_hasDefaultValue;
        private Func<object> m_defaultValueFactory;
        private Func<ITypeProcessor> m_typeProcessorFactory;
        private List<PropertyInfo> m_changeWithProperties;

        public PropertyInfo Property { get; private set; }

        public PropertyMap<TEntity, TProperty> Identity()
        {
            this.m_isIdentity = true;
            return this;
        }

        public PropertyMap<TEntity, TProperty> Name(string name)
        {
            this.m_name = name;
            return this;
        }

        public PropertyMap<TEntity, TProperty> DefaultValue(TProperty value)
        {
            this.m_hasDefaultValue = true;
            this.m_defaultValueFactory = () => value;
            return this;
        }

        public PropertyMap<TEntity, TProperty> ChangeWith<TWith>(Expression<Func<TEntity, TWith>> withProperty)
        {
            this.m_changeWithProperties.Add((PropertyInfo)(withProperty.Body as MemberExpression).Member);
            return this;
        }

        public PropertyMap<TEntity, TProperty> Processor<TProcesspr>()
            where TProcesspr : ITypeProcessor, new()
        {
            this.m_typeProcessorFactory = () => new TProcesspr();
            return this;
        }

        public PropertyMap<TEntity, TProperty> Processor(ITypeProcessor processor)
        {
            this.m_typeProcessorFactory = () => processor;
            return this;
        }

        IPropertyDescriptor IPropertyMap.ToDescriptor()
        {
            return new PropertyDescriptor
            {
                IsIdentity = this.m_isIdentity,
                DefaultValueFactory = this.m_defaultValueFactory,
                HasDefaultValue = this.m_hasDefaultValue,
                Name = this.m_name ?? this.Property.Name,
                Property = this.Property,
                ChangeWithProperties = new ReadOnlyCollection<PropertyInfo>(this.m_changeWithProperties),
                TypeProcessorFactory = this.m_typeProcessorFactory ?? (() => null)
            };
        }
    }
}

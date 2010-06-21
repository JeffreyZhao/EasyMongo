using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace EasyMongo.Reflection
{
    internal class PropertyAccessor
    {
        public PropertyAccessor(PropertyInfo property)
        {
            this.Property = property;

            if (property.CanRead)
            {
                // instance
                var instanceExpr = Expression.Parameter(typeof(object), "instance");
                // (InstanceType)instance
                var typedInstanceExpr = Expression.Convert(instanceExpr, property.DeclaringType);
                // ((InstanceType)instance).Property
                var propertyExpr = Expression.Property(typedInstanceExpr, property);
                // (object)((InstanceType)instance).Property
                var objectExpr = Expression.Convert(propertyExpr, typeof(object));
                // instance => (object)((InstanceType)instance).Property
                var lambdaExpr = Expression.Lambda<Func<object, object>>(objectExpr, instanceExpr);

                this.m_getValue = lambdaExpr.Compile();
            }

            if (property.CanWrite)
            {
                var method = property.GetSetMethod();
                
                // instance
                var instanceExpr = Expression.Parameter(typeof(object), "instance");
                // value
                var valueExpr = Expression.Parameter(typeof(object), "value");
                // (InstanceType)instance
                var typedInstanceExpr = Expression.Convert(instanceExpr, property.DeclaringType);
                // (ValueType)value
                var typedValueExpr = Expression.Convert(valueExpr, method.GetParameters()[0].ParameterType);
                // ((InstanceType)instance).set_Property((ValueType)value)
                var setPropertyExpr = Expression.Call(typedInstanceExpr, method, typedValueExpr);
                // (instance, value) => ((InstanceType)instance).set_Property((ValueType)value)
                var lambdaExpr = Expression.Lambda<Action<object, object>>(setPropertyExpr, instanceExpr, valueExpr);

                this.m_setValue = lambdaExpr.Compile();
            }
        }

        public PropertyInfo Property { get; private set; }

        private Func<object, object> m_getValue;
        private Action<object, object> m_setValue;

        public object GetValue(object instance)
        {
            return this.m_getValue(instance);
        }

        public void SetValue(object instance, object value)
        {
            this.m_setValue(instance, value);
        }
    }
}

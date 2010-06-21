using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace EasyMongo.Reflection
{
    internal class EntityFactory
    {
        public EntityFactory(Type type)
        {
            this.Type = type;

            this.m_create =
                Expression.Lambda<Func<object>>(
                    Expression.Convert(
                        Expression.New(type),
                        typeof(object))).Compile();
        }

        public Type Type { get; private set; }

        private Func<object> m_create;

        public object Create()
        {
            return this.m_create();
        }
    }
}

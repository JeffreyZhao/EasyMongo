using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace EasyMongo.Types
{
    internal class BasicProcessor : ITypeProcessor
    {
        public virtual object ToDocumentValue(object value)
        {
            return value;
        }

        public virtual object FromDocumentValue(object docValue)
        {
            return docValue;
        }

        public virtual object ToStateValue(object value)
        {
            return value;
        }

        public bool IsStateChanged(object originalState, object currentState)
        {
            return !Object.Equals(originalState, currentState);
        }

        public object FromStateValue(object stateValue)
        {
            return stateValue;
        }
    }
}

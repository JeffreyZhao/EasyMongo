using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo.Types
{
    public interface ITypeProcessor
    {
        object ToDocumentValue(object value);

        object FromDocumentValue(object docValue);

        object ToStateValue(object value);

        object FromStateValue(object stateValue);

        bool IsStateChanged(object originalState, object currentState);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo.Types
{
    internal interface IArrayProcessor : ITypeProcessor
    {
        object[] GetItemsToPush(object originalState, object currentState);
        object Create(params object[] items);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo.Mapping
{
    internal interface IEntityMap
    {
        IEntityDescriptor ToDescriptor();
    }
}

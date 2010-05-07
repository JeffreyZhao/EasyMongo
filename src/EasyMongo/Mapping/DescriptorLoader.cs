using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace EasyMongo.Mapping
{
    public static class DescriptorLoader
    {
        public static ReadOnlyCollection<IEntityDescriptor> Load(Assembly assembly)
        {
            var descriptors =
                from t in assembly.GetTypes()
                where typeof(IEntityMap).IsAssignableFrom(t)
                let map = (IEntityMap)Activator.CreateInstance(t)
                select map.ToDescriptor();

            return new ReadOnlyCollection<IEntityDescriptor>(descriptors.ToList());
        }
    }
}

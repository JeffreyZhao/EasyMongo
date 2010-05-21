using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace EasyMongo.Types
{
    internal class ArrayState
    {
        public ArrayState(IList container)
        { 
            this.Container = container;
            this.Items = container.Cast<object>().ToList();
        }

        public IList Container { get; private set; }

        public List<object> Items { get; set; }
    }
}

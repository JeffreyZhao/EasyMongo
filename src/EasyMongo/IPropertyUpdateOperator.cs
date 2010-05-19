using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace EasyMongo
{
    internal interface IPropertyUpdateOperator
    {
        void PutConstantUpdate(Document doc, object value);

        void PutAddUpdate(Document doc, object value);

        void PutSubtractUpdate(Document doc, object value);

        void PutPushUpdate(Document doc, IEnumerable<object> values);
    }
}

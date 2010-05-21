using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace EasyMongo
{
    internal interface IPropertyPredicateOperator
    {
        void PutEqualPredicate(Document doc, object value);
        void PutGreaterThanPredicate(Document doc, object value);
        void PutGreaterThanOrEqualPredicate(Document doc, object value);
        void PutLessThanPredicate(Document doc, object value);
        void PutLessThanOrEqualPredicate(Document doc, object value);
        void PutContainsPredicate(Document doc, object value);
        void PutContainedInPredicate(Document doc, IEnumerable<object> collections);
    }
}

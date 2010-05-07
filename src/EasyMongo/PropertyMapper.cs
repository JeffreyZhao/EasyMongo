using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EasyMongo
{
    internal class PropertyMapper
    {
        public PropertyMapper(IPropertyDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }
        
        public IPropertyDescriptor Descriptor { get; private set; }

        public void FillEqualPredicate(Document doc, object value)
        {
            var name = this.Descriptor.Name;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Append(name, value);
        }

        public void FillGreaterThanPredicate(Document doc, object value)
        {
            this.FillInnerPredicate(doc, "$gt", value);
        }

        private void FillInnerPredicate(Document doc, string op, object value)
        {
            var name = this.Descriptor.Name;
            Document innerDoc;

            if (doc.Contains(name))
            {
                innerDoc = doc[name] as Document;
                if (innerDoc == null)
                {
                    throw new InvalidOperationException("Should have nothing or Document object");
                }
            }
            else
            {
                innerDoc = new Document();
                doc.Append(name, innerDoc);
            }

            innerDoc.Append(op, value);
        }

        public void FillGreaterThanOrEqualPredicate(Document doc, object value)
        {
            this.FillInnerPredicate(doc, "$gte", value);
        }

        public void FillLessThanPredicate(Document doc, object value)
        {
            this.FillInnerPredicate(doc, "$lt", value);
        }

        public void FillLessThanOrEqualPredicate(Document doc, object value)
        {
            this.FillInnerPredicate(doc, "$lte", value);
        }
    }
}

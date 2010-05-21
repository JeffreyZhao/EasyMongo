using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using EasyMongo.Types;

namespace EasyMongo
{
    internal class PropertyMapper : IPropertyUpdateOperator, IPropertyPredicateOperator
    {
        public PropertyMapper(IPropertyDescriptor descriptor, bool isDefaultIdentity)
        {
            this.Descriptor = descriptor;
            this.IsDefaultIdentity = isDefaultIdentity;

            var type = descriptor.Property.PropertyType;
            if (type.IsEnum)
            {
                this.Type = new EnumType();
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                this.Type = new ArrayType();
            }
            else
            {
                this.Type = new NormalType();
            }
        }

        public IType Type { get; private set; }
        
        public IPropertyDescriptor Descriptor { get; private set; }

        public bool IsDefaultIdentity { get; private set; }

        public string DatabaseName
        {
            get
            {
                if (this.IsDefaultIdentity) return "_id";

                return this.Descriptor.Name;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                var descriptor = this.Descriptor;
                return descriptor.IsIdentity || descriptor.ChangeWithProperties.Count > 0;
            }
        }

        private object ToDocumentValue(object value)
        {
            return this.Type.ToDocumentValue(this.Descriptor.Property, value);
        }

        private object FromDocumentValue(object value)
        {
            return this.Type.FromDocumentValue(this.Descriptor.Property, value);
        }

        public void PutValue(Document target, object sourceEntity)
        {
            var value = this.Descriptor.Property.GetValue(sourceEntity, null);
            target.Append(this.DatabaseName, this.ToDocumentValue(value));
        }

        public void PutField(Document doc, bool include)
        {
            var name = this.DatabaseName;
            if (!doc.Contains(name))
            {
                doc.Append(name, include ? 1 : 0);
            }
        }

        public void PutState(Dictionary<IPropertyDescriptor, object> targetState, object sourceEntity)
        {
            var name = this.DatabaseName;
            var property = this.Descriptor.Property;
            var type = property.PropertyType;

            object value = property.GetValue(sourceEntity, null);
            if (typeof(IList).IsAssignableFrom(type) && value != null) // is array
            {
                value = new ArrayState((IList)value);
            }

            targetState.Add(this.Descriptor, value);
        }

        public void SetValue(object targetEntity, Document sourceDoc)
        {
            var name = this.DatabaseName;

            object value;
            if (sourceDoc.Contains(name))
            {
                var docValue = sourceDoc[name];
                if (docValue == MongoDBNull.Value) docValue = null;
                value = this.FromDocumentValue(docValue);
            }
            else if (this.Descriptor.HasDefaultValue)
            {
                value = this.Descriptor.GetDefaultValue();
            }
            else
            {
                throw new ArgumentException("Missing the value of " + name);
            }

            this.Descriptor.Property.SetValue(targetEntity, value, null);
        }

        public bool IsChanged(
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var name = this.DatabaseName;
            var property = this.Descriptor.Property;
            var type = property.PropertyType;

            var originalValue = originalState[this.Descriptor];
            var currentValue = currentState[this.Descriptor];

            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                var originalArray = (ArrayState)originalValue;
                var currentArray = (ArrayState)currentValue;

                if (currentArray == null && originalArray != null)
                {
                    return true;
                }
                else if (currentArray != null && originalArray == null)
                {
                    return true;
                }
                else if (!Object.ReferenceEquals(originalArray.Container, currentArray.Container))
                {
                    return true;
                }
                else if (currentArray.Items.Count != originalArray.Items.Count)
                {
                    return true;
                }
                else
                {
                    for (int i = 0; i < currentArray.Items.Count; i++)
                    {
                        if (currentArray.Items[0] != originalArray.Items[0])
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            else
            {
                return !originalValue.Equals(currentValue);
            }
        }

        public void PutValueChange(Document targetDoc, object sourceEntity)
        {
            Document innerDoc;
            if (targetDoc.Contains("$set"))
            {
                innerDoc = (Document)targetDoc["$set"];
            }
            else
            {
                innerDoc = new Document();
                innerDoc.Append("$set", innerDoc);
            }

            this.PutValue(innerDoc, sourceEntity);
        }

        public void PutStateChange(
            Document targetDoc,
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var name = this.DatabaseName;
            var property = this.Descriptor.Property;
            var type = property.PropertyType;

            var originalValue = originalState[this.Descriptor];
            var currentValue = currentState[this.Descriptor];

            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                var originalArray = (ArrayState)originalValue;
                var currentArray = (ArrayState)currentValue;

                if (currentArray == null && originalArray != null)
                {
                    this.AppendOperation(targetDoc, "$set", null);
                }
                else if (currentArray != null && originalArray == null)
                {
                    var value = currentArray.Items.ToArray();
                    this.AppendOperation(targetDoc, "$set", value);
                }
                else if (!Object.ReferenceEquals(originalArray.Container, currentArray.Container))
                {
                    var value = currentArray.Items.ToArray();
                    this.AppendOperation(targetDoc, "$set", value);
                }
                else if (originalArray.Items.Count > currentArray.Items.Count)
                {
                    throw new NotSupportedException("Does not support item removal in array.");
                }
                else
                {
                    for (var i = 0; i < originalArray.Items.Count; i++)
                    {
                        if (originalArray.Items[i] != currentArray.Items[i])
                        {
                            throw new NotSupportedException("Does not support item removal in array.");
                        }
                    }

                    var itemAdded = currentArray.Items.Skip(originalArray.Items.Count).ToArray();

                    if (itemAdded.Length > 0)
                    {
                        this.AppendOperation(targetDoc, "$pushAll", itemAdded);
                    }
                }
            }
            else if (!originalValue.Equals(currentValue))
            {
                object value;

                if (type.IsEnum)
                {
                    if (type.IsDefined(typeof(FlagsAttribute), false))
                    {
                        value = currentValue.ToString().Split(new[] { ", " }, StringSplitOptions.None);
                    }
                    else
                    {
                        value = currentValue.ToString();
                    }
                }
                else
                {
                    value = currentValue;
                }

                this.AppendOperation(targetDoc, "$set", value);
            }
        }

        private void AppendOperation(Document doc, string op, object value)
        {
            Document innerDoc;
            if (doc.Contains(op))
            {
                innerDoc = (Document)doc[op];
            }
            else
            {
                innerDoc = new Document();
                doc.Append(op, innerDoc);
            }

            innerDoc.Append(this.DatabaseName, value);
        }

        public void PutSortOrder(Document doc, bool descending)
        {
            doc.Append(this.DatabaseName, descending ? -1 : 1);
        }

        #region IPropertyUpdateOperator members

        void IPropertyPredicateOperator.PutEqualPredicate(Document doc, object value)
        {
            var name = this.DatabaseName;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            doc.Append(name, this.ToDocumentValue(value));
        }

        private void PutInnerPredicate(Document doc, string op, object value)
        {
            var name = this.DatabaseName;
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

            innerDoc.Append(op, this.ToDocumentValue(value));
        }

        void IPropertyPredicateOperator.PutGreaterThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gt", value);
        }

        void IPropertyPredicateOperator.PutGreaterThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gte", value);
        }

        void IPropertyPredicateOperator.PutLessThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lt", value);
        }

        void IPropertyPredicateOperator.PutLessThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lte", value);
        }

        void IPropertyPredicateOperator.PutContainsPredicate(Document doc, object value)
        {
            doc.Append(this.DatabaseName, value);
        }

        void IPropertyPredicateOperator.PutContainedInPredicate(Document doc, IEnumerable<object> collections)
        {
            var name = this.DatabaseName;
            if (doc.Contains(name))
            {
                throw new InvalidOperationException(
                    String.Format(
                        "this document should not contain {0} field.", name));
            }

            var array = collections.Select(this.ToDocumentValue).ToArray();
            doc.Append(this.DatabaseName, new Document().Append("$in", array));
        }

        #endregion

        #region IPropertyUpdateOperator members

        void IPropertyUpdateOperator.PutConstantUpdate(Document doc, object value)
        {
            this.AppendOperation(doc, "$set", this.ToDocumentValue(value));
        }

        void IPropertyUpdateOperator.PutAddUpdate(Document doc, object value)
        {
            this.AppendOperation(doc, "$inc", this.ToDocumentValue(value));
        }

        void IPropertyUpdateOperator.PutSubtractUpdate(Document doc, object value)
        {
            ((IPropertyUpdateOperator)this).PutAddUpdate(doc, -(int)value);
        }

        void IPropertyUpdateOperator.PutPushUpdate(Document doc, object value)
        {
            this.AppendOperation(doc, "$push", this.ToDocumentValue(value));
        }

        #endregion
    }
}

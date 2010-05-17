using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;

namespace EasyMongo
{
    internal class PropertyMapper
    {
        public PropertyMapper(IPropertyDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }
        
        public IPropertyDescriptor Descriptor { get; private set; }

        public bool IsReadOnly { get { return this.Descriptor.ChangeWithProperties.Count > 0; } }

        public void PutEqualPredicate(Document doc, object value)
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

        public void PutGreaterThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gt", value);
        }

        private void PutInnerPredicate(Document doc, string op, object value)
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

        public void PutGreaterThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$gte", value);
        }

        public void PutLessThanPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lt", value);
        }

        public void PutLessThanOrEqualPredicate(Document doc, object value)
        {
            this.PutInnerPredicate(doc, "$lte", value);
        }

        public void PutValue(Document target, object sourceEntity)
        {
            object docValue;

            var property = this.Descriptor.Property;
            var type = property.PropertyType;
            var value = property.GetValue(sourceEntity, null);

            if (typeof(IList).IsAssignableFrom(type))
            {
                docValue = value == null ? null : ((IList)value).Cast<object>().ToArray();
            }
            else if (type.IsEnum)
            {
                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    docValue = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
                }
                else
                {
                    docValue = value.ToString();
                }
            }
            else
            {
                docValue = value;
            }

            target.Append(this.Descriptor.Name, docValue);
        }

        public void PutField(Document doc)
        {
            doc.Append(this.Descriptor.Name, 1);
        }

        public void PutState(Dictionary<IPropertyDescriptor, object> targetState, object sourceEntity)
        {
            var name = this.Descriptor.Name;
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
            var name = this.Descriptor.Name;

            object docValue;
            if (sourceDoc.Contains(name))
            {
                docValue = sourceDoc[name];
                if (docValue == MongoDBNull.Value) docValue = null;
            }
            else if (this.Descriptor.HasDefaultValue)
            {
                docValue = this.Descriptor.DefaultValue;
            }
            else
            {
                throw new ArgumentException("Missing the value of " + name);
            }

            var property = this.Descriptor.Property;
            var type = property.PropertyType;
            object value;

            if (typeof(IList).IsAssignableFrom(type)) // is array
            {
                if (docValue == null)
                {
                    value = null;
                }
                else
                {
                    var list = (IList)Activator.CreateInstance(type);
                    if (!(docValue is Document)) // not empty array
                    {
                        foreach (var item in ((IEnumerable)docValue)) list.Add(item);
                    }

                    value = list;
                }
            }
            else if (type.IsEnum)
            {
                if (docValue == null)
                {
                    throw new ArgumentException("Enum value cannot be assigned to null for " + name);
                }

                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    if (docValue is Document) // empty array;
                    {
                        value = Enum.Parse(type, "");
                    }
                    else
                    {
                        var array = ((IEnumerable)docValue).Cast<string>().ToArray();
                        value = Enum.Parse(type, String.Join(", ", array));
                    }
                }
                else
                {
                    value = Enum.Parse(type, docValue.ToString());
                }
            }
            else
            {
                value = docValue;
            }

            property.SetValue(targetEntity, value, null);
        }

        public bool IsChanged(
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var name = this.Descriptor.Name;
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

        public void PutStateChange(
            Document targetDoc,
            Dictionary<IPropertyDescriptor, object> originalState,
            Dictionary<IPropertyDescriptor, object> currentState)
        {
            var name = this.Descriptor.Name;
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

            innerDoc.Append(this.Descriptor.Name, value);
        }

        public void PutSortOrder(Document doc, bool descending)
        {
            doc.Append(this.Descriptor.Name, descending ? -1 : 1);
        }
    }
}

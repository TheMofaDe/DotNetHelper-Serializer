using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public sealed class XmlEnumItemCollection : IEnumerable<XmlEnumItem>
    {
        private readonly List<XmlEnumItem> items;

        public XmlEnumItemCollection()
        {
            items = new List<XmlEnumItem>();
        }

        public XmlEnumItemCollection(IEnumerable<XmlEnumItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.items = new List<XmlEnumItem>(items);
        }

        public int Count => items.Count;

        public void Add(long value, string name)
        {
            var item = new XmlEnumItem(value, name);
            Add(item);
        }

        public void Add(XmlEnumItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var index = IndexOf(item.Value);

            if (index != -1)
            {
                throw new ArgumentException(string.Format("Enum item \"{0}\" allready registered.", item.Value), nameof(item));
            }

            items.Add(item);
        }

        public void Set(XmlEnumItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var index = IndexOf(item.Value);

            if (index == -1)
            {
                items.Add(item);
            }
            else
            {
                items[index] = item;
            }
        }

        public bool Contains(long value)
        {
            return IndexOf(value) != -1;
        }

        public bool Remove(long value)
        {
            var index = IndexOf(value);

            if (index != -1)
            {
                items.RemoveAt(index);
                return true;
            }

            return false;
        }

        public IEnumerator<XmlEnumItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        private int IndexOf(long value)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Value == value)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
using System;
using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public sealed class XmlName : IEquatable<XmlName>
    {
        public XmlName(string localName)
            : this(localName, null)
        {
        }

        public XmlName(string localName, string namespaceUri)
        {
            if (localName == null)
            {
                throw new ArgumentNullException(nameof(localName));
            }

            XmlConvert.VerifyNCName(localName);

            if (namespaceUri != null)
            {
                XmlNamespace.VerifyNamespaceUri(namespaceUri);
            }

            this.LocalName = localName;
            this.NamespaceUri = namespaceUri;
        }

        public string LocalName { get; }

        public string NamespaceUri { get; }

        public static implicit operator XmlName(string name)
        {
            return new XmlName(name);
        }

        public override string ToString()
        {
            if (NamespaceUri != null)
            {
                return "{" + NamespaceUri + "}" + LocalName;
            }

            return LocalName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XmlName);
        }

        public override int GetHashCode()
        {
            var hashCode = 17;

            unchecked
            {
                hashCode = 31 * hashCode + LocalName.GetHashCode();

                if (NamespaceUri != null)
                {
                    hashCode = 31 * hashCode + NamespaceUri.GetHashCode();
                }
            }

            return hashCode;
        }

        public bool Equals(XmlName other)
        {
            if (other == null)
            {
                return false;
            }

            return LocalName == other.LocalName && NamespaceUri == other.NamespaceUri;
        }

        internal XmlName Create(string localName, string namespaceUri)
        {
            if (string.IsNullOrEmpty(localName))
            {
                localName = this.LocalName;
            }

            if (string.IsNullOrEmpty(namespaceUri))
            {
                namespaceUri = this.NamespaceUri;
            }

            return new XmlName(localName, namespaceUri);
        }
    }
}
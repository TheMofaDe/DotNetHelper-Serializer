using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public sealed class XmlCustomContractResolver : IXmlContractResolver
    {
        private readonly IXmlContractResolver fallbackResolver;
        private readonly Dictionary<Type, XmlContract> contracts;

        public XmlCustomContractResolver(IEnumerable<XmlContract> contracts)
            : this(contracts, new XmlContractResolver())
        {
        }

        public XmlCustomContractResolver(IEnumerable<XmlContract> contracts, IXmlContractResolver fallbackResolver)
        {
            if (contracts == null)
            {
                throw new ArgumentNullException(nameof(contracts));
            }

            this.fallbackResolver = fallbackResolver;
            this.contracts = new Dictionary<Type, XmlContract>();

            foreach (var contract in contracts)
            {
                this.contracts.Add(contract.ValueType, contract);
            }
        }

        public IEnumerable<XmlContract> Contracts => contracts.Values;

        public XmlContract ResolveContract(Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }


            if (!contracts.TryGetValue(valueType, out XmlContract contract))
            {
                contract = fallbackResolver?.ResolveContract(valueType) ?? throw new XmlSerializationException(string.Format("Can't resolve contract for \"{0}\".", valueType));
            }

            return contract;
        }
    }
}
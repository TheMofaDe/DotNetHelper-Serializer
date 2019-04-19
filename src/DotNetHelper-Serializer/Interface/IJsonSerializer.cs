using DotNetHelper_IO.Interface;
using Newtonsoft.Json;

namespace DotNetHelper_Serializer.Interface
{
    public interface IJsonSerializer : ISerializer
    {
        JsonSerializerSettings Settings { get; set; }

        /// <summary>
        /// Serialize Json to xml 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="xmlSerializer">xml serialization settings</param>
        /// <returns></returns>
        string SerializeToXml(string json, IXmlSerializer xmlSerializer);

    }

}

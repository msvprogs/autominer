using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Msv.Licensing.Common
{
    public class CorrectXmlSerializer<T> : ISerializer<T>
    {
        // XmlSerializer doesn't work properly - it throws 'Invalid path' exception on the internal call
        private readonly DataContractSerializer m_Serializer = new DataContractSerializer(typeof(T));

        public string Serialize(T value)
        {
            if (value == null) 
                throw new ArgumentNullException(nameof(value));

            using (var stringWriter = new StringWriter())
            using (var writer = new XmlTextWriter(stringWriter))
            {
                m_Serializer.WriteObject(writer, value);
                return stringWriter.ToString();
            }
        }

        public T Deserialize(string serialized)
        {
            if (serialized == null) 
                throw new ArgumentNullException(nameof(serialized));

            using (var stringReader = new StringReader(serialized))
            using (var reader = new XmlTextReader(stringReader))
                return (T)m_Serializer.ReadObject(reader);
        }
    }
}

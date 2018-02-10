using System;
using System.IO;
using System.Xml.Serialization;

namespace Msv.Licensing.Common
{
    public class CorrectXmlSerializer<T> : ISerializer<T>
    {
        private readonly XmlSerializer m_Serializer = new XmlSerializer(typeof(T));

        public string Serialize(T value)
        {
            if (value == null) 
                throw new ArgumentNullException(nameof(value));

            using (var writer = new StringWriter())
            {
                m_Serializer.Serialize(writer, value);
                return writer.ToString();
            }
        }

        public T Deserialize(string serialized)
        {
            if (serialized == null) 
                throw new ArgumentNullException(nameof(serialized));

            using (var reader = new StringReader(serialized))
                return (T)m_Serializer.Deserialize(reader);
        }
    }
}

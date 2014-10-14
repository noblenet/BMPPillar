using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PseudoKlient
{
    public class NNSUtilities
    {
        public static Object DeserializeObject(string serializedObject, Type objectType)
        {
            var xs = new XmlSerializer(objectType);
            var encoding = new UTF8Encoding();
            var memoryStream = new MemoryStream(encoding.GetBytes(serializedObject));
            var xmlTestWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return xs.Deserialize(memoryStream);
        }

        public static string Serialize(object msgObject)
        {
            var xmls = new XmlSerializer(msgObject.GetType());
            using (var ms = new MemoryStream())
            {
                var utf = new UTF8Encoding(false);
                var settings = new XmlWriterSettings
                    {
                        Encoding = utf,
                        Indent = true,
                        IndentChars = "  ",
                        NewLineChars = Environment.NewLine,
                        ConformanceLevel = ConformanceLevel.Document,
                        OmitXmlDeclaration = false
                    };

                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    xmls.Serialize(writer, msgObject);
                }
                string serializedObject = Encoding.UTF8.GetString(ms.ToArray());
                return serializedObject;
            }
        }
    }
}
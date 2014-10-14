using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BMPInterface
{
    public class NNSUtilities
    {
        public static Object DeserializeObject(string serializedObject, Type objectType)
        {
            XmlSerializer xs = new XmlSerializer(objectType);
            UTF8Encoding encoding = new UTF8Encoding();
            MemoryStream memoryStream = new MemoryStream(encoding.GetBytes(serializedObject));
            XmlTextWriter xmlTestWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return xs.Deserialize(memoryStream);
        }

        public static string Serialize(object msgObject)
        {
            XmlSerializer xmls = new XmlSerializer(msgObject.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utf = new UTF8Encoding(false);
                XmlWriterSettings settings = new XmlWriterSettings
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
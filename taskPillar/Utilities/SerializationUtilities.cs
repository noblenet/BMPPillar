using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using log4net;

namespace PillarAPI.Utilities
{
    public class SerializationUtilities
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Object DeserializeObject(string serializedObject, Type objectType)
        {
            var xs = new XmlSerializer(objectType);
            var encoding = new UTF8Encoding();
            var memoryStream = new MemoryStream(encoding.GetBytes(serializedObject));
            object a = xs.Deserialize(memoryStream);
            memoryStream.Dispose();
            return a;
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

        public static bool ValidateXmlMessage(string message)
        {
            bool isValid = true;
            try
            {
                var settings = new XmlReaderSettings();
                //settings.Schemas.Add(null, Pillar.GlobalPillarApiSettings.MESSAGE_XSD_FILE_PATH);
                settings.Schemas.Add("http://bitrepository.org/BitRepositoryMessages.xsd",
                                     Pillar.GlobalPillarApiSettings.MESSAGE_XSD_FILE_PATH);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                //settings.ValidationEventHandler += ValidationCallBack;
                //settings.ValidationEventHandler += ValidationCallBack;
                settings.ValidationType = ValidationType.Schema;
                var document = new XmlDocument();
                document.LoadXml(message);
                try
                {
                    using (XmlReader xmlRdr = XmlReader.Create(new StringReader(document.InnerXml), settings))
                    {
                        while (xmlRdr.Read())
                        {
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Debug(e.ToString());
                    isValid = false;
                }
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                isValid = false;
            }
            return isValid;
        }
    }
}
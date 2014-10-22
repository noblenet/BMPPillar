using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using log4net;

namespace PillarAPI.Utilities
{
    public class XmlUtilities
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Uses XSD file to validate the XML message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static bool ValidateXmlMessage(string message)
        {
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(Pillar.GlobalPillarApiSettings.XML_NAMESPACE,
                                 Pillar.GlobalPillarApiSettings.MESSAGE_XSD_FILE_PATH);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
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
                return false;
            }
            return true;
        }

        public static bool ValidateFileChkSum(IEnumerable<byte> chksumFromClient, IEnumerable<byte> calculatedChksum)
        {
            return (chksumFromClient.SequenceEqual(calculatedChksum));
        }
    }
}
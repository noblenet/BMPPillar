﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 
namespace pillarAPI {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class PillarApiSettings {
        
        private string aLARM_DESTINATIONField;
        
        private string aLLOWED_FILE_ID_PATTERNField;
        
        private string cOLLECTION_DESTINATIONField;
        
        private string cOLLECTION_FILE_DIRECTORYField;
        
        private string cOLLECTION_IDField;
        
        private decimal cOLLECTION_SETTINGS_VERSIONField;
        
        private string cOLLECTION_SETTINGS_XML_FILE_PATHField;
        
        private string cOLLECTION_SETTINGS_XSD_FILE_PATHField;
        
        private string cONTRIBUTOR_IDField;
        
        private string dEFAULT_CHECKSUM_TYPEField;
        
        private string dRIVES4STORAGEField;
        
        private string gENERAL_TOPIC_SUBSCRIBERField;
        
        private string mESSAGE_BUS_CONFIGURATION_URLField;
        
        private string mESSAGE_XSD_FILE_PATHField;
        
        private string mIN_MESSAGE_XSD_VERSIONField;
        
        private string pILLAR_IDField;
        
        private string pRIVATE_CERTIFICATE_THUMBPRINTField;
        
        private string pUBLIC_CERTIFICATE_THUMBPRINTField;
        
        private string rEQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTSField;
        
        private string rEQUERED_CHECKSUM_FOR_NEW_FILE_REQUESTField;
        
        private string rEQUERED_MESSAGE_AUTHENTICATIONField;
        
        private string rEQUERED_OPERATION_AUTHENTICATIONField;
        
        private string sA_PILLAR_QUEUEField;
        
        private string sA_QUEUE_SUBSCRIBERField;
        
        private string sQLITE_CONNECTION_STRINGField;
        
        private string uSER_CERTIFICATES_STOREField;
        
        private string wEBDAV_BASEFOLDERNAMEField;
        
        private string wEBDAV_CLIENT_CERTIFICATE_THUMBPRINTField;
        
        private int wEBDAV_HTTP_PORTField;
        
        private string wEBDAV_IP_ADDRESSField;
        
        private string wEBDAV_URI_SCHEMEField;
        
        private string xML_NAMESPACEField;
        
        private string xSD_VERSIONField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string ALARM_DESTINATION {
            get {
                return this.aLARM_DESTINATIONField;
            }
            set {
                this.aLARM_DESTINATIONField = value;
            }
        }
        
        /// <remarks/>
        public string ALLOWED_FILE_ID_PATTERN {
            get {
                return this.aLLOWED_FILE_ID_PATTERNField;
            }
            set {
                this.aLLOWED_FILE_ID_PATTERNField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string COLLECTION_DESTINATION {
            get {
                return this.cOLLECTION_DESTINATIONField;
            }
            set {
                this.cOLLECTION_DESTINATIONField = value;
            }
        }
        
        /// <remarks/>
        public string COLLECTION_FILE_DIRECTORY {
            get {
                return this.cOLLECTION_FILE_DIRECTORYField;
            }
            set {
                this.cOLLECTION_FILE_DIRECTORYField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string COLLECTION_ID {
            get {
                return this.cOLLECTION_IDField;
            }
            set {
                this.cOLLECTION_IDField = value;
            }
        }
        
        /// <remarks/>
        public decimal COLLECTION_SETTINGS_VERSION {
            get {
                return this.cOLLECTION_SETTINGS_VERSIONField;
            }
            set {
                this.cOLLECTION_SETTINGS_VERSIONField = value;
            }
        }
        
        /// <remarks/>
        public string COLLECTION_SETTINGS_XML_FILE_PATH {
            get {
                return this.cOLLECTION_SETTINGS_XML_FILE_PATHField;
            }
            set {
                this.cOLLECTION_SETTINGS_XML_FILE_PATHField = value;
            }
        }
        
        /// <remarks/>
        public string COLLECTION_SETTINGS_XSD_FILE_PATH {
            get {
                return this.cOLLECTION_SETTINGS_XSD_FILE_PATHField;
            }
            set {
                this.cOLLECTION_SETTINGS_XSD_FILE_PATHField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string CONTRIBUTOR_ID {
            get {
                return this.cONTRIBUTOR_IDField;
            }
            set {
                this.cONTRIBUTOR_IDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string DEFAULT_CHECKSUM_TYPE {
            get {
                return this.dEFAULT_CHECKSUM_TYPEField;
            }
            set {
                this.dEFAULT_CHECKSUM_TYPEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NMTOKEN")]
        public string DRIVES4STORAGE {
            get {
                return this.dRIVES4STORAGEField;
            }
            set {
                this.dRIVES4STORAGEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string GENERAL_TOPIC_SUBSCRIBER {
            get {
                return this.gENERAL_TOPIC_SUBSCRIBERField;
            }
            set {
                this.gENERAL_TOPIC_SUBSCRIBERField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string MESSAGE_BUS_CONFIGURATION_URL {
            get {
                return this.mESSAGE_BUS_CONFIGURATION_URLField;
            }
            set {
                this.mESSAGE_BUS_CONFIGURATION_URLField = value;
            }
        }
        
        /// <remarks/>
        public string MESSAGE_XSD_FILE_PATH {
            get {
                return this.mESSAGE_XSD_FILE_PATHField;
            }
            set {
                this.mESSAGE_XSD_FILE_PATHField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="integer")]
        public string MIN_MESSAGE_XSD_VERSION {
            get {
                return this.mIN_MESSAGE_XSD_VERSIONField;
            }
            set {
                this.mIN_MESSAGE_XSD_VERSIONField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string PILLAR_ID {
            get {
                return this.pILLAR_IDField;
            }
            set {
                this.pILLAR_IDField = value;
            }
        }
        
        /// <remarks/>
        public string PRIVATE_CERTIFICATE_THUMBPRINT {
            get {
                return this.pRIVATE_CERTIFICATE_THUMBPRINTField;
            }
            set {
                this.pRIVATE_CERTIFICATE_THUMBPRINTField = value;
            }
        }
        
        /// <remarks/>
        public string PUBLIC_CERTIFICATE_THUMBPRINT {
            get {
                return this.pUBLIC_CERTIFICATE_THUMBPRINTField;
            }
            set {
                this.pUBLIC_CERTIFICATE_THUMBPRINTField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string REQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTS {
            get {
                return this.rEQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTSField;
            }
            set {
                this.rEQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTSField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string REQUERED_CHECKSUM_FOR_NEW_FILE_REQUEST {
            get {
                return this.rEQUERED_CHECKSUM_FOR_NEW_FILE_REQUESTField;
            }
            set {
                this.rEQUERED_CHECKSUM_FOR_NEW_FILE_REQUESTField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string REQUERED_MESSAGE_AUTHENTICATION {
            get {
                return this.rEQUERED_MESSAGE_AUTHENTICATIONField;
            }
            set {
                this.rEQUERED_MESSAGE_AUTHENTICATIONField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string REQUERED_OPERATION_AUTHENTICATION {
            get {
                return this.rEQUERED_OPERATION_AUTHENTICATIONField;
            }
            set {
                this.rEQUERED_OPERATION_AUTHENTICATIONField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string SA_PILLAR_QUEUE {
            get {
                return this.sA_PILLAR_QUEUEField;
            }
            set {
                this.sA_PILLAR_QUEUEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string SA_QUEUE_SUBSCRIBER {
            get {
                return this.sA_QUEUE_SUBSCRIBERField;
            }
            set {
                this.sA_QUEUE_SUBSCRIBERField = value;
            }
        }
        
        /// <remarks/>
        public string SQLITE_CONNECTION_STRING {
            get {
                return this.sQLITE_CONNECTION_STRINGField;
            }
            set {
                this.sQLITE_CONNECTION_STRINGField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string USER_CERTIFICATES_STORE {
            get {
                return this.uSER_CERTIFICATES_STOREField;
            }
            set {
                this.uSER_CERTIFICATES_STOREField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string WEBDAV_BASEFOLDERNAME {
            get {
                return this.wEBDAV_BASEFOLDERNAMEField;
            }
            set {
                this.wEBDAV_BASEFOLDERNAMEField = value;
            }
        }
        
        /// <remarks/>
        public string WEBDAV_CLIENT_CERTIFICATE_THUMBPRINT {
            get {
                return this.wEBDAV_CLIENT_CERTIFICATE_THUMBPRINTField;
            }
            set {
                this.wEBDAV_CLIENT_CERTIFICATE_THUMBPRINTField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="int")]
        public int WEBDAV_HTTP_PORT {
            get {
                return this.wEBDAV_HTTP_PORTField;
            }
            set {
                this.wEBDAV_HTTP_PORTField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NMTOKEN")]
        public string WEBDAV_IP_ADDRESS {
            get {
                return this.wEBDAV_IP_ADDRESSField;
            }
            set {
                this.wEBDAV_IP_ADDRESSField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="NCName")]
        public string WEBDAV_URI_SCHEME {
            get {
                return this.wEBDAV_URI_SCHEMEField;
            }
            set {
                this.wEBDAV_URI_SCHEMEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string XML_NAMESPACE {
            get {
                return this.xML_NAMESPACEField;
            }
            set {
                this.xML_NAMESPACEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string XSD_VERSION {
            get {
                return this.xSD_VERSIONField;
            }
            set {
                this.xSD_VERSIONField = value;
            }
        }
    }
}

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PillarAPI.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("queue://sa_pillar_queue")]
        public string SA_PILLAR_QUEUE {
            get {
                return ((string)(this["SA_PILLAR_QUEUE"]));
            }
            set {
                this["SA_PILLAR_QUEUE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ssl://217.198.211.150:61616?transport.clientcertfilename=D:/Downloads/Apache/apac" +
            "he-activemq-5.5.1/conf/client.cer&transport.acceptInvalidBrokerCert=true")]
        public string MESSAGE_BUS_CONFIGURATION_URL {
            get {
                return ((string)(this["MESSAGE_BUS_CONFIGURATION_URL"]));
            }
            set {
                this["MESSAGE_BUS_CONFIGURATION_URL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("kbpillar2")]
        public string PILLAR_ID {
            get {
                return ((string)(this["PILLAR_ID"]));
            }
            set {
                this["PILLAR_ID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("general_topic_subscriber")]
        public string GENERAL_TOPIC_SUBSCRIBER {
            get {
                return ((string)(this["GENERAL_TOPIC_SUBSCRIBER"]));
            }
            set {
                this["GENERAL_TOPIC_SUBSCRIBER"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("sa_topic_subscriber")]
        public string SA_QUEUE_SUBSCRIBER {
            get {
                return ((string)(this["SA_QUEUE_SUBSCRIBER"]));
            }
            set {
                this["SA_QUEUE_SUBSCRIBER"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=D:\\Udvikling\\bmpSQLite.s3db;Version=3;LockingMode=Exclusive;Pooling=T" +
            "rue;MaxPoolSize=200;")]
        public string SQLITE_CONNECTION_STRING {
            get {
                return ((string)(this["SQLITE_CONNECTION_STRING"]));
            }
            set {
                this["SQLITE_CONNECTION_STRING"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("topic://integrationtest1")]
        public string COLLECTION_DESTINATION {
            get {
                return ((string)(this["COLLECTION_DESTINATION"]));
            }
            set {
                this["COLLECTION_DESTINATION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\collectionFiles\\")]
        public string COLLECTION_FILE_DIRECTORY {
            get {
                return ((string)(this["COLLECTION_FILE_DIRECTORY"]));
            }
            set {
                this["COLLECTION_FILE_DIRECTORY"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Udvikling\\Kode\\xsdFiles\\bitrepository-message-xml-24\\xsd\\BitRepositoryMessages" +
            ".xsd")]
        public string MESSAGE_XSD_FILE_PATH {
            get {
                return ((string)(this["MESSAGE_XSD_FILE_PATH"]));
            }
            set {
                this["MESSAGE_XSD_FILE_PATH"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("17")]
        public string XSD_VERSION {
            get {
                return ((string)(this["XSD_VERSION"]));
            }
            set {
                this["XSD_VERSION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("17")]
        public string MIN_MESSAGE_XSD_VERSION {
            get {
                return ((string)(this["MIN_MESSAGE_XSD_VERSION"]));
            }
            set {
                this["MIN_MESSAGE_XSD_VERSION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MD5")]
        public string DEFAULT_CHECKSUM_TYPE {
            get {
                return ((string)(this["DEFAULT_CHECKSUM_TYPE"]));
            }
            set {
                this["DEFAULT_CHECKSUM_TYPE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("@\"^[a-zA-Z_\\.\\-0-9]+$\"")]
        public string ALLOWED_FILE_ID_PATTERN {
            get {
                return ((string)(this["ALLOWED_FILE_ID_PATTERN"]));
            }
            set {
                this["ALLOWED_FILE_ID_PATTERN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool REQUERED_MESSAGE_AUTHENTICATION {
            get {
                return ((bool)(this["REQUERED_MESSAGE_AUTHENTICATION"]));
            }
            set {
                this["REQUERED_MESSAGE_AUTHENTICATION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool REQUERED_OPERATION_AUTHENTICATION {
            get {
                return ((bool)(this["REQUERED_OPERATION_AUTHENTICATION"]));
            }
            set {
                this["REQUERED_OPERATION_AUTHENTICATION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool REQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTS {
            get {
                return ((bool)(this["REQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTS"]));
            }
            set {
                this["REQUERED_CHECKSUM_FOR_DESTRUCTIVE_REQUESTS"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool REQUERED_CHECKSUM_FOR_NEW_FILE_REQUEST {
            get {
                return ((bool)(this["REQUERED_CHECKSUM_FOR_NEW_FILE_REQUEST"]));
            }
            set {
                this["REQUERED_CHECKSUM_FOR_NEW_FILE_REQUEST"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("topic://sa-test.alarms")]
        public string ALARM_DESTINATION {
            get {
                return ((string)(this["ALARM_DESTINATION"]));
            }
            set {
                this["ALARM_DESTINATION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("sa-test-pillar")]
        public string CONTRIBUTOR_ID {
            get {
                return ((string)(this["CONTRIBUTOR_ID"]));
            }
            set {
                this["CONTRIBUTOR_ID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("integrationtest1")]
        public string COLLECTION_ID {
            get {
                return ((string)(this["COLLECTION_ID"]));
            }
            set {
                this["COLLECTION_ID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.9")]
        public string COLLECTION_SETTINGS_VERSION {
            get {
                return ((string)(this["COLLECTION_SETTINGS_VERSION"]));
            }
            set {
                this["COLLECTION_SETTINGS_VERSION"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string COLLECTION_SETTINGS_XML_FILE_PATH {
            get {
                return ((string)(this["COLLECTION_SETTINGS_XML_FILE_PATH"]));
            }
            set {
                this["COLLECTION_SETTINGS_XML_FILE_PATH"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Udvikling\\Kode\\TestTing\\xsdFiles\\bitrepository-collection-settings-0.9\\xsd")]
        public string COLLECTION_SETTINGS_XSD_FILE_PATH {
            get {
                return ((string)(this["COLLECTION_SETTINGS_XSD_FILE_PATH"]));
            }
            set {
                this["COLLECTION_SETTINGS_XSD_FILE_PATH"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://bitrepository.org/BitRepositoryMessages.xsd")]
        public string XML_NAMESPACE {
            get {
                return ((string)(this["XML_NAMESPACE"]));
            }
            set {
                this["XML_NAMESPACE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B60843F8A3BFBD2944EAF9043D9C2443B936A981")]
        public string PRIVATE_CERTIFICATE_THUMBPRINT {
            get {
                return ((string)(this["PRIVATE_CERTIFICATE_THUMBPRINT"]));
            }
            set {
                this["PRIVATE_CERTIFICATE_THUMBPRINT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Root")]
        public string USER_CERTIFICATES_STORE {
            get {
                return ((string)(this["USER_CERTIFICATES_STORE"]));
            }
            set {
                this["USER_CERTIFICATES_STORE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B60843F8A3BFBD2944EAF9043D9C2443B936A981")]
        public string PUBLIC_CERTIFICATE_THUMBPRINT {
            get {
                return ((string)(this["PUBLIC_CERTIFICATE_THUMBPRINT"]));
            }
            set {
                this["PUBLIC_CERTIFICATE_THUMBPRINT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("U:")]
        public string DRIVES4STORAGE {
            get {
                return ((string)(this["DRIVES4STORAGE"]));
            }
            set {
                this["DRIVES4STORAGE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("dav")]
        public string WEBDAV_BASEFOLDERNAME {
            get {
                return ((string)(this["WEBDAV_BASEFOLDERNAME"]));
            }
            set {
                this["WEBDAV_BASEFOLDERNAME"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("147cafe2c7d0b07d3a61653977a87ff7fe91f5c6")]
        public string WEBDAV_CLIENT_CERTIFICATE_THUMBPRINT {
            get {
                return ((string)(this["WEBDAV_CLIENT_CERTIFICATE_THUMBPRINT"]));
            }
            set {
                this["WEBDAV_CLIENT_CERTIFICATE_THUMBPRINT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("217.198.211.150")]
        public string WEBDAV_IP_ADDRESS {
            get {
                return ((string)(this["WEBDAV_IP_ADDRESS"]));
            }
            set {
                this["WEBDAV_IP_ADDRESS"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("443")]
        public int WEBDAV_HTTP_PORT {
            get {
                return ((int)(this["WEBDAV_HTTP_PORT"]));
            }
            set {
                this["WEBDAV_HTTP_PORT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("HTTPS")]
        public string WEBDAV_URI_SCHEME {
            get {
                return ((string)(this["WEBDAV_URI_SCHEME"]));
            }
            set {
                this["WEBDAV_URI_SCHEME"] = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RoslynPad
{
    [XmlRoot("Plugin", IsNullable = false)]
    public class PluginDescriptor
    {
        public PluginDescriptor()
        {
            Type = "";
            Name = "";
            Assembly = "";
        }
        [XmlAttribute("type")]
        public string Type
        {
            get;set;
        }
        [XmlAttribute("name")]
        public string Name
        {
            get;set;
        }

        [XmlAttribute("assembly")]
        public string Assembly
        {
            get;set;
        }
    }
    [XmlInclude(typeof(PluginDescriptor))]
    public class PluginConfiguration
    {
        [XmlElement("Plugin")]
        public PluginDescriptor[]? Plugins;
    }
    public class XMLSerializerSectionHandler : IConfigurationSectionHandler
    {
        // Methods
        public object Create(object parent, object configContext, XmlNode section)
        {
            XmlSerializer serializer = new XmlSerializer(Type.GetType(section.CreateNavigator().Evaluate("string(@type)").ToString()));
            return serializer.Deserialize(new XmlNodeReader(section));
        }
    }
}

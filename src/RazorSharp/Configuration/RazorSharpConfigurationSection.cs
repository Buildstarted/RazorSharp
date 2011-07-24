using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace RazorSharp.Configuration {
    public class RazorSharpConfigurationSection : ConfigurationSection {
        private const string NamespacesElement = "namespaces";
        private const string SectionPath = "RazorSharp";

        [ConfigurationProperty(NamespacesElement)]
        public NamespaceConfigurationElementCollection Namespaces {
            get { return (NamespaceConfigurationElementCollection)this[NamespacesElement]; }
            set { this[NamespacesElement] = value; }
        }

        public static RazorSharpConfigurationSection GetConfiguration() {
            return ConfigurationManager.GetSection(SectionPath) as RazorSharpConfigurationSection;
        }
    }
}

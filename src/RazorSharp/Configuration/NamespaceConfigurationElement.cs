using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace RazorSharp.Configuration {
    public class NamespaceConfigurationElement : ConfigurationElement {
        #region Fields
        private const string NamespaceAttribute = "namespace";
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        [ConfigurationProperty(NamespaceAttribute, IsRequired = true, IsKey = true)]
        public string Namespace {
            get { return (string)this[NamespaceAttribute]; }
            set { this[NamespaceAttribute] = value; }
        }
        #endregion
    }
}

namespace RazorSharp.Configuration
{
    using System.Configuration;

    public class RazorSharpConfigurationSection : ConfigurationSection
    {
        private const string NamespacesElement = "namespaces";
        private const string SectionPath = "RazorSharp";

        [ConfigurationProperty(NamespacesElement)]
        public NamespaceConfigurationElementCollection Namespaces
        {
            get { return (NamespaceConfigurationElementCollection) this[NamespacesElement]; }
            set { this[NamespacesElement] = value; }
        }

        public static RazorSharpConfigurationSection GetConfiguration()
        {
            return ConfigurationManager.GetSection(SectionPath) as RazorSharpConfigurationSection
                ?? new RazorSharpConfigurationSection();
        }
    }
}
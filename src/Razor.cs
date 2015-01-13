namespace RazorSharp
{
    public class Razor
    {
        internal static RazorCompiler compiler;

        static Razor()
        {
            compiler = new RazorCompiler();
        }

        public static string Parse(string template)
        {
            return compiler.Render(template).Result;
        }

        public static string Parse(string template, string name)
        {
            return compiler.Render(template, name).Result;
        }

        public static string Parse(string template, string name, string masterTemplate)
        {
            return compiler.Render(template, name, masterTemplate).Result;
        }

        public static string Parse<T>(T model, string template)
        {
            return compiler.Render(model, template).Result;
        }

        public static string Parse<T>(T model, string template, string name)
        {
            return compiler.Render(model, template, name).Result;
        }

        public static string Parse<T>(T model, string template, string name, string masterTemplate)
        {
            return compiler.Render(model, template, name, masterTemplate).Result;
        }
    }
}
namespace RazorSharp
{
    using System;

    public class Parser
    {
        private readonly RazorCompiler _compiler;

        public Parser() : this(typeof (TemplateBase<>))
        {
        }

        public Parser(Type baseType)
        {
            _compiler = new RazorCompiler(baseType);
        }

        public virtual string Parse(string template)
        {
            return _compiler.Render(template).Result;
        }

        public virtual string Parse(string template, string name)
        {
            return _compiler.Render(template, name).Result;
        }

        public virtual string Parse(string template, string name, string masterTemplate)
        {
            return _compiler.Render(template, name, masterTemplate).Result;
        }

        public virtual string Parse<T>(T model, string template)
        {
            return _compiler.Render(model, template).Result;
        }

        public virtual string Parse<T>(T model, string template, string name)
        {
            return _compiler.Render(model, template, name).Result;
        }

        public virtual string Parse<T>(T model, string template, string name, string masterTemplate)
        {
            return _compiler.Render(model, template, name, masterTemplate).Result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorSharp {
    public class Parser {
        private RazorCompiler _compiler;

        public Parser() : this(typeof(TemplateBase<>)) { }

        public Parser(Type baseType) {
            _compiler = new RazorCompiler(baseType);
        }

        public virtual string Parse(string template) {
            return _compiler.Render(template).Result;
        }

        public virtual string Parse(string template, string name) {
            return _compiler.Render(template, name).Result;
        }

        public virtual string Parse(string template, string name, string masterTemplate) {
            return _compiler.Render(template, name, masterTemplate).Result;
        }

        public virtual string Parse<T>(T model, string template) {
            return _compiler.Render<T>(model, template).Result;
        }

        public virtual string Parse<T>(T model, string template, string name) {
            return _compiler.Render<T>(model, template, name).Result;
        }

        public virtual string Parse<T>(T model, string template, string name, string masterTemplate) {
            return _compiler.Render<T>(model, template, name, masterTemplate).Result;
        }

    }
}

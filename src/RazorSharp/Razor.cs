using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorSharp {
    public class Razor {
        public static string Parse(string template) {
            return RazorCompiler.Render(template).Result;
        }

        public static string Parse(string template, string name) {
            return RazorCompiler.Render(template, name).Result;
        }

        public static string Parse(string template, string name, string masterTemplate) {
            return RazorCompiler.Render(template, name, masterTemplate).Result;
        }

        public static string Parse<T>(T model, string template) {
            return RazorCompiler.Render<T>(model, template).Result;
        }

        public static string Parse<T>(T model, string template, string name) {
            return RazorCompiler.Render<T>(model, template, name).Result;
        }

        public static string Parse<T>(T model, string template, string name, string masterTemplate) {
            return RazorCompiler.Render<T>(model, template, name, masterTemplate).Result;
        }

    }
}

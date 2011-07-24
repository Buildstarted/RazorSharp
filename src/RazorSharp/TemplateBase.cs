using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;

namespace RazorSharp {
    public abstract class TemplateBase : RazorSharp.ITemplateBase {

        protected TemplateBase() {
            Builder = new StringBuilder();
            Razor = new RazorHelper(this);
        }

        public Func<string> RenderBody { get; set; }

        public virtual RazorHelper Razor { get; set; }
        public string Source { get; set; }
        public bool Cached { get; set; }
        public string Name { get; set; }
        public StringBuilder Builder { get; private set; }
        public string Result { get { return Builder.ToString(); } }

        public void Clear() {
            Builder.Clear();
        }

        public virtual void Execute() { }

        public void Write(object @object) {
            if (@object == null)
                return;

            Builder.Append(@object);
        }

        public void WriteLiteral(string @string) {
            if (@string == null)
                return;

            Builder.Append(@string);
        }

        public static void WriteLiteralTo(TextWriter writer, string literal) {
            if (literal == null)
                return;

            writer.Write(literal);
        }


        public static void WriteTo(TextWriter writer, object obj) {
            if (obj == null)
                return;

            writer.Write(obj);
        }

    }

    public abstract class TemplateBase<T> : TemplateBase {

        public new RazorHelper<T> Razor { get; set; }

        object model;
        public T Model {
            get { return (T)model; }
            set {
                if (typeof(T) == typeof(object) && !(value is DynamicObject) && !(value is ExpandoObject))
                    model = new RazorDynamicObject { Model = value };
                else
                    model = value;
            }
        }
    }

}

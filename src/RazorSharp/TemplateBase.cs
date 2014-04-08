namespace RazorSharp
{
    using System;
    using System.Dynamic;
    using System.IO;
    using System.Text;

    public abstract class TemplateBase : ITemplateBase
    {
        public Func<string> RenderBody { get; set; }
        public virtual RazorHelper Razor { get; set; }
        public string Source { get; set; }
        public bool Cached { get; set; }
        public string Name { get; set; }
        public StringBuilder Builder { get; private set; }

        protected TemplateBase()
        {
            Builder = new StringBuilder();
            Razor = new RazorHelper(this);
        }

        public string Result
        {
            get { return Builder.ToString(); }
        }

        public void Clear()
        {
            Builder.Clear();
        }

        public virtual void Execute()
        {
        }

        public void Write(object @object)
        {
            if (@object == null)
            {
                return;
            }

            Builder.Append(@object);
        }

        public void WriteLiteral(string @string)
        {
            if (@string == null)
            {
                return;
            }

            Builder.Append(@string);
        }

        public void WriteAttribute(TextWriter writer, string @string)
        {
            Builder.Append(@string);
        }

        public virtual void WriteAttribute(string name, Tuple<string, int> prefix, Tuple<string, int> suffix, params Tuple<Tuple<string, int>, Tuple<object, int>, bool>[] values)
        {
            Builder.Append(prefix.Item1);

            if (values.Length > 0)
            {
                foreach (var value in values)
                {
                    Builder.Append(value.Item2.Item1);
                    Builder.Append(' ');
                }

                Builder.Length -= 1;
            }

            Builder.Append(suffix.Item1);
        }

        public static void WriteLiteralTo(TextWriter writer, string literal)
        {
            if (literal == null)
            {
                return;
            }

            writer.Write(literal);
        }


        public static void WriteTo(TextWriter writer, object obj)
        {
            if (obj == null)
            {
                return;
            }

            writer.Write(obj);
        }
    }

    public abstract class TemplateBase<T> : TemplateBase
    {
        private object _model;
        public new RazorHelper<T> Razor { get; set; }

        public T Model
        {
            get { return (T) _model; }
            set
            {
                if (   typeof (T) == typeof (object)
                    && !(value is DynamicObject)
                    && !(value is ExpandoObject))
                {
                    _model = new RazorDynamicObject { Model = value };
                }
                else
                {
                    _model = value;
                }
            }
        }
    }
}
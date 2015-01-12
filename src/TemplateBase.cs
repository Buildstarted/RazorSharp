namespace RazorSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

        public virtual void WriteAttribute(string name, Tuple<string, int> prefix, Tuple<string, int> suffix, params object[] values)
        {
            Builder.Append(prefix.Item1);

            foreach (var value in values)
            {
                var valueType = Helpers.TypeName(value.GetType());

                Func<object, string> func;
                if (!StringifierDict.TryGetValue(valueType, out func))
                {
                    Debug.WriteLine("Can't find stringifier for type {0}", (object) valueType);
                    Builder.Append("");
                    continue;
                }

                Builder.Append(func(value));
            }

            Builder.Append(suffix.Item1);
        }

        private static readonly Dictionary<string, Func<object, string>> StringifierDict = new Dictionary<string, Func<object, string>>
        {
            { "Tuple<Tuple<String, Int32>, Tuple<Object, Int32>, Boolean>", t => Stringifier(t as Tuple<Tuple<string, int>, Tuple<object, int>, bool>) },
            { "Tuple<Tuple<String, Int32>, Tuple<String, Int32>, Boolean>", t => Stringifier(t as Tuple<Tuple<string, int>, Tuple<string, int>, bool>) }
        };

        private static string Stringifier(Tuple<Tuple<string, int>, Tuple<object, int>, bool> t)
        {
            return HtmlAttributeEncode(t.Item2.Item1.ToString());
        }

        private static string Stringifier(Tuple<Tuple<string, int>, Tuple<string, int>, bool> t)
        {
            return HtmlAttributeEncode(t.Item2.Item1);
        }

        private static string HtmlAttributeEncode(string rawAttribute)
        {
            StringBuilder builder = new StringBuilder();

            for (int stringIndex = 0;
                 stringIndex < rawAttribute.Length;
                 stringIndex++)
            {
                char rawAttributeChar = rawAttribute[stringIndex];
                switch (rawAttributeChar)
                {
                    case '"':

                        builder.Append("&quot;");
                        break;

                    case '&':

                        builder.Append("&amp;");
                        break;

                    case '<':

                        builder.Append("&lt;");
                        break;

                    default:

                        builder.Append(rawAttributeChar);
                        break;
                }
            }

            return builder.ToString();
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
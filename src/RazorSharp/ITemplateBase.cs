using System;
namespace RazorSharp {
    public interface ITemplateBase {
        System.Text.StringBuilder Builder { get; }
        void Clear();
        void Execute();
        string Name { get; set; }
        RazorHelper Razor { get; set; }
        Func<string> RenderBody { get; set; }
        string Result { get; }
        string Source { get; set; }
        bool Cached { get; set; }
        void Write(object @object);
        void WriteLiteral(string @string);
    }
}

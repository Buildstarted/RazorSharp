namespace RazorSharp
{
    using System;
    using System.Text;

    public interface ITemplateBase
    {
        StringBuilder Builder { get; }
        string Name { get; set; }
        RazorHelper Razor { get; set; }
        Func<string> RenderBody { get; set; }
        string Result { get; }
        string Source { get; set; }
        bool Cached { get; set; }
        void Clear();
        void Execute();
        void Write(object @object);
        void WriteLiteral(string @string);
    }
}
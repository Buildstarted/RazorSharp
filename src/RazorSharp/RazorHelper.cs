namespace RazorSharp
{
    public class RazorHelper
    {
        private readonly ITemplateBase _template;

        public RazorHelper(ITemplateBase templateBase)
        {
            _template = templateBase;
        }

        public string Name
        {
            get { return _template.Name; }
        }
    }

    public class RazorHelper<T> : RazorHelper
    {
        public RazorHelper(ITemplateBase templateBase) : base(templateBase)
        {
        }
    }
}
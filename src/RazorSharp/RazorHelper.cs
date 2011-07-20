using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorSharp {
    public class RazorHelper {

        ITemplateBase template;

        public string Name {
            get {
                return template.Name;
            }
        }

        public RazorHelper(ITemplateBase templateBase) {
            this.template = templateBase;
        }
    }
}

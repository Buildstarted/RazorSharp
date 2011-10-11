using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RazorSharp.Tests {
    public class Blah {
        public string ChildTemplate { get; set; }
    }
  
    [TestClass]
    public class ExtensionTests {
        [TestMethod]
        public void include_adds_template_to_existing_template() {
            //force load
            AppDomain.CurrentDomain.Load("RazorSharp.Extensions");
            string parentTemplate =
                "This is the parent template > @Razor.Include(Model.ChildTemplate) < contained is the child template";
            string childTemplate =
                "This is the child template";

            string result = Razor.Parse(new Blah {ChildTemplate = childTemplate}, parentTemplate);

            Assert.IsTrue(result.Contains(childTemplate));
        }
    }
}

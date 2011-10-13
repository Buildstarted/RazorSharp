using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RazorSharp.Tests {
    [TestClass]
    public class Tests {
        string Name = "Some User";

        [TestMethod]
        public void razor_parser_with_anonymouse_type() {
            string razor = "@Model.Name";

            string result = RazorSharp.Razor.Parse(new { Name = Name }, razor);
            Assert.AreEqual(Name, result);
        }

        [TestMethod]
        public void razor_parser_with_concrete_type() {
            string razor = "@Model.Name";

            string result = RazorSharp.Razor.Parse(new User { Name = Name }, razor);
            Assert.AreEqual(Name, result);
        }

        [TestMethod]
        public void razor_parser_with_master_template() {
            string razor = "@Model.Name";
            string master = "Master Page\r\n@RenderBody()";

            string result = RazorSharp.Razor.Parse(new { Name = Name }, razor, master);
            Assert.IsTrue(result.Contains(Name));
            Assert.IsTrue(result.Contains("Master Page"));
        }

        [TestMethod]
        public void razor_parser_with_name_results_in_subsequent_calls_from_cache() {
            string razor = "@Model.Name";
            string cacheName = "username";

            string result = RazorSharp.Razor.Parse(new { Name = Name }, razor, cacheName, null);
            Assert.AreEqual(Name, result);
            var instance = RazorSharp.Razor.compiler.Render(new { Name = Name }, razor, cacheName, null); 
            Assert.AreEqual(instance.Cached, true);
        }

        [TestMethod]
        public void razor_parser_with_mastertemplate_and_name_results_in_subsequent_calls_from_cache() {
            string razor = "@Model.Name";
            string master = "Master Page\r\n@RenderBody()";
            string cacheName = "username_master";

            string result = RazorSharp.Razor.Parse(new { Name = Name }, razor, cacheName, master);
            Assert.IsTrue(result.Contains(Name));
            Assert.IsTrue(result.Contains("Master Page"));

            var instance = RazorSharp.Razor.compiler.Render(new { Name = Name }, razor, cacheName, master);
            Assert.AreEqual(true, instance.Cached);
            Assert.AreEqual(cacheName + "_masterTemplate", instance.Name);
        }
    }
}
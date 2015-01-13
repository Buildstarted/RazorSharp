namespace RazorSharp.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class Tests
    {
        const string UserName = "Some User";

        [Test]
        public void razor_parser_with_anonymouse_type()
        {
            const string template = "@Model.Name";

            var result = Razor.Parse(new { Name = UserName }, template);
            Assert.AreEqual(UserName, result);
        }

        [Test]
        public void razor_parser_with_concrete_type()
        {
            const string template = "@Model.Name";

            var result = Razor.Parse(new User { Name = UserName }, template);
            Assert.AreEqual(UserName, result);
        }

        [Test]
        public void razor_parser_with_master_template()
        {
            const string template = "@Model.Name";
            const string masterTemplate = "Master Page\r\n@RenderBody()";

            var result = Razor.Parse(new { Name = UserName }, template, null, masterTemplate);
            Assert.IsTrue(result.Contains(UserName));
            Assert.IsTrue(result.Contains("Master Page"));
        }

        [Test]
        public void razor_parser_with_name_results_in_subsequent_calls_from_cache()
        {
            const string template = "@Model.Name";
            const string cacheName = "username";

            var result = Razor.Parse(new { Name = UserName }, template, cacheName, null);
            Assert.AreEqual(UserName, result);
            var instance = Razor.compiler.Render(new { Name = UserName }, template, cacheName, null); 
            Assert.AreEqual(instance.Cached, true);
        }

        [Test]
        public void razor_parser_with_mastertemplate_and_name_results_in_subsequent_calls_from_cache()
        {
            const string template = "@Model.Name";
            const string masterTemplate = "Master Page\r\n@RenderBody()";
            const string cacheName = "username_master";

            var result = Razor.Parse(new { Name = UserName }, template, cacheName, masterTemplate);
            Assert.IsTrue(result.Contains(UserName));
            Assert.IsTrue(result.Contains("Master Page"));

            var instance = Razor.compiler.Render(new { Name = UserName }, template, cacheName, masterTemplate);
            Assert.AreEqual(true, instance.Cached);
            Assert.AreEqual(cacheName + "_masterTemplate", instance.Name);
        }
    }
}
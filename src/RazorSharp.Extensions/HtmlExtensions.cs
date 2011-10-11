using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RazorSharp.Extensions {
    public static class HtmlExtensions {

        /// <summary>
        /// Include adds the results of the passed template to the output
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="helper"></param>
        /// <param name="model"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static string Include<TModel>(this RazorHelper<TModel> helper, TModel model, string template) {
            return RazorCompiler.Render<TModel>(model, template, null).Result;
        }

        public static string Include(this RazorHelper helper, string template) {
            return RazorCompiler.Render(template).Result;
        }
    }
}

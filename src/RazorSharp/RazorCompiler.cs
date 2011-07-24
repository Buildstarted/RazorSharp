using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Runtime.CompilerServices;
using System.Dynamic;
using RazorSharp.Configuration;

namespace RazorSharp {
    public class RazorCompiler {

        //cache of already compiled types
        static ConcurrentDictionary<string, Type> cache = new ConcurrentDictionary<string, Type>();

        static RazorCompiler() {
            bool loaded = typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly != null;
        }

        #region " Static Renders "
        public static ITemplateBase Render(string razorTemplate, string masterTemplate = null) {
            return Render(razorTemplate, null, masterTemplate: masterTemplate);
        }

        public static ITemplateBase Render(string razorTemplate, string name, string masterTemplate = null) {
            return Render(new object(), razorTemplate, name, masterTemplate: masterTemplate);
        }

        public static ITemplateBase Render<T>(T model, string razorTemplate, string masterTemplate = null) {
            return Render<T>(model, razorTemplate, null, masterTemplate: masterTemplate);
        }

        public static ITemplateBase Render<T>(T model, string razorTemplate, string name, string masterTemplate = null) {
            var instance = GetCompiledTemplate<T>(model, razorTemplate, name);

            if (typeof(T) != typeof(object)) {
                if (IsDynamicType(typeof(T))) {
                    ((TemplateBase<dynamic>)instance).Model = model;
                } else {
                    ((TemplateBase<T>)instance).Model = model;
                }
            }

            instance.Execute();

            if (masterTemplate != null) {
                return RenderMasterView<T>(model, masterTemplate, instance);
            } else {
                return instance;
            }
        }
        #endregion

        //gets the pre-compiled template - or else compiles it
        private static ITemplateBase GetCompiledTemplate<T>(T model, string razorTemplate, string name) {
            Type type;
            bool isCached = false;

            if (name == null) {
                type = GetCompiledType<T>(razorTemplate);
            } else {
                isCached = cache.TryGetValue(name, out type);

                if (!isCached) {
                    type = GetCompiledType<T>(razorTemplate);
                    cache[name] = type;
                }
            }


            var instance = (ITemplateBase)Activator.CreateInstance(type);
            ((ITemplateBase)instance).Source = razorTemplate;
            ((ITemplateBase)instance).Cached = isCached;
            instance.Name = name;
            return instance;
        }

        //used to render the master
        private static ITemplateBase RenderMasterView<T>(T model, string template, ITemplateBase instance) {
            var masterInstance = GetCompiledTemplate<object>(model, template, instance.Name + "_masterTemplate");
            //RenderBody is a func that we can overwrite
            masterInstance.RenderBody = () => {
                return instance.Result;
            };

            masterInstance.Execute();

            return masterInstance;
        }

        private static Type GetCompiledType<T>(string template) {
            var key = "c" + Guid.NewGuid().ToString("N");

            var parser = new HtmlMarkupParser();

            var baseType = typeof(TemplateBase<>);// IsDynamicType(typeof(T)) ? typeof(TemplateBase<>).MakeGenericType(typeof(T)) : typeof(TemplateBase<>);

            var regex = new System.Text.RegularExpressions.Regex("@model.*");
            template = regex.Replace(template, "");

            var host = new RazorEngineHost(new System.Web.Razor.CSharpRazorCodeLanguage(), () => parser) {
                DefaultBaseClass = BuildTypeName(baseType, typeof(T)),
                DefaultClassName = key,
                DefaultNamespace = "RazorSharp.Dynamic",
                GeneratedClassContext = new GeneratedClassContext("Execute", "Write", "WriteLiteral", "WriteTo", "WriteLiteralTo", "RazorSharp.TemplateBase")
            };

            //always include this one
            host.NamespaceImports.Add("RazorSharp");
            host.NamespaceImports.Add("System");

            var config = Configuration.RazorSharpConfigurationSection.GetConfiguration();
            if (config.Namespaces != null && config.Namespaces.Count > 0) {
                foreach (NamespaceConfigurationElement @namespace in config.Namespaces) {
                    host.NamespaceImports.Add(@namespace.Namespace);
                }
            }

            //read web.config pages/namespaces
            //replace this later
            //if (System.IO.File.Exists("\\web.config")) {
            //    var config = WebConfigurationManager.OpenWebConfiguration("\\web.config");
            //    var pages = config.GetSection("system.web/pages");
            //    if (pages != null) {
            //        System.Web.Configuration.PagesSection pageSection = (System.Web.Configuration.PagesSection)pages;
            //        for (int i = 0; i < pageSection.Namespaces.Count; i++) {
            //            //this automatically ignores namespaces already added
            //            host.NamespaceImports.Add(pageSection.Namespaces[i].Namespace);
            //        }
            //    }
            //}

            CodeCompileUnit code;
            using (var reader = new StringReader(template)) {
                var generatedCode = new RazorTemplateEngine(host).GenerateCode(reader);
                code = generatedCode.GeneratedCode;
            }

            var @params = new CompilerParameters {
                IncludeDebugInformation = false,
                TempFiles = new TempFileCollection(AppDomain.CurrentDomain.DynamicDirectory),
                CompilerOptions = "/target:library /optimize",
                GenerateInMemory = false
            };

            var assemblies = AppDomain.CurrentDomain
               .GetAssemblies()
               .Where(a => !a.IsDynamic)
               .Select(a => a.Location)
               .ToArray();

            @params.ReferencedAssemblies.AddRange(assemblies);

            var provider = new CSharpCodeProvider();
            var compiled = provider.CompileAssemblyFromDom(@params, code);

            if (compiled.Errors.Count > 0) {
                var compileErrors = string.Join("\r\n", compiled.Errors.Cast<object>().Select(o => o.ToString()));
                throw new ApplicationException("Failed to compile Razor:" + compileErrors);
            }

            return compiled.CompiledAssembly.GetType("RazorSharp.Dynamic." + key);
        }

        internal static bool IsAnonymousType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return (type.IsClass
                    && type.IsSealed
                    && type.BaseType == typeof(object)
                    && type.Name.StartsWith("<>")
                    && type.IsDefined(typeof(CompilerGeneratedAttribute), true));
        }

        internal static bool IsDynamicType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return (typeof(DynamicObject).IsAssignableFrom(type)
                    || typeof(ExpandoObject).IsAssignableFrom(type)
                    || IsAnonymousType(type));
        }

        internal static string BuildTypeNameInternal(Type type, bool isDynamic) {
            if (!type.IsGenericType)
                return type.FullName;

            return type.Namespace
                   + "."
                   + type.Name.Substring(0, type.Name.IndexOf('`'))
                   + "<"
                   + (isDynamic ? "dynamic" : string.Join(", ", type.GetGenericArguments().Select(t => BuildTypeNameInternal(t, IsDynamicType(t)))))
                   + ">";
        }

        internal static string BuildTypeName(Type templateType, Type modelType) {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            if (!templateType.IsGenericTypeDefinition && !templateType.IsGenericType)
                return templateType.FullName;

            if (modelType == null)
                throw new ArgumentException("The template type is a generic defintion, and no model type has been supplied.");

            bool @dynamic = IsDynamicType(modelType);
            Type genericType = templateType.MakeGenericType(IsDynamicType(modelType) ? typeof(object) : modelType);

            return BuildTypeNameInternal(genericType, @dynamic);
        }
    }
}

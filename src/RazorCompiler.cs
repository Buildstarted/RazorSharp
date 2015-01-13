namespace RazorSharp
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Concurrent;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser;
    using Configuration;
    using Microsoft.CSharp;
    using Microsoft.CSharp.RuntimeBinder;

    public class RazorCompiler
    {
        //cache of already compiled types
        private static readonly ConcurrentDictionary<string, Type> cache = new ConcurrentDictionary<string, Type>();
        private readonly Type _baseType;

        static RazorCompiler()
        {
            // This line of code is essential!
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once UnusedVariable

            var loaded = typeof (Binder).Assembly != null;
        }

        public RazorCompiler() : this(typeof (TemplateBase<>))
        {
        }

        public RazorCompiler(Type baseType)
        {
            _baseType = baseType;
        }

        #region Static Renders

        public ITemplateBase Render(string razorTemplate, string masterTemplate = null)
        {
            return Render(razorTemplate, null, masterTemplate);
        }

        public ITemplateBase Render(string razorTemplate, string name, string masterTemplate = null)
        {
            return Render(new object(), razorTemplate, name, masterTemplate);
        }

        public ITemplateBase Render<T>(T model, string razorTemplate, string masterTemplate = null)
        {
            return Render(model, razorTemplate, null, masterTemplate);
        }

        public ITemplateBase Render<T>(T model, string razorTemplate, string name, string masterTemplate = null)
        {
            ITemplateBase instance = GetCompiledTemplate(model, razorTemplate, name, _baseType);

            if (typeof (T) != typeof (object))
            {
                if (IsDynamicType(typeof (T)))
                {
                    ((TemplateBase<dynamic>) instance).Model = model;
                }
                else
                {
                    ((TemplateBase<T>) instance).Model = model;
                }
            }

            instance.Execute();

            if (masterTemplate != null)
            {
                return RenderMasterView(model, masterTemplate, instance);
            }

            return instance;
        }

        #endregion

        //gets the pre-compiled template - or else compiles it
        private static ITemplateBase GetCompiledTemplate<T>(T model, string razorTemplate, string name, Type baseType)
        {
            Type type;
            var isCached = false;

            if (name == null)
            {
                type = GetCompiledType<T>(razorTemplate, baseType);
            }
            else
            {
                isCached = cache.TryGetValue(name, out type);

                if (!isCached)
                {
                    type = GetCompiledType<T>(razorTemplate, baseType);
                    cache[name] = type;
                }
            }


            var instance = (ITemplateBase) Activator.CreateInstance(type);
            instance.Source = razorTemplate;
            instance.Cached = isCached;
            instance.Name = name;
            return instance;
        }

        //used to render the master
        private static ITemplateBase RenderMasterView<T>(T model, string template, ITemplateBase instance)
        {
            ITemplateBase masterInstance = GetCompiledTemplate<object>(model, template,
                instance.Name + "_masterTemplate", instance.GetType());
            //RenderBody is a func that we can overwrite
            masterInstance.RenderBody = () => { return instance.Result; };

            masterInstance.Execute();

            return masterInstance;
        }

        private static Type GetCompiledType<T>(string template, Type baseType)
        {
            string key = "c" + Guid.NewGuid().ToString("N");

            var parser = new HtmlMarkupParser();

            //var baseType = typeof(TemplateBase<>);// IsDynamicType(typeof(T)) ? typeof(TemplateBase<>).MakeGenericType(typeof(T)) : typeof(TemplateBase<>);

            var regex = new Regex("@model.*");
            template = regex.Replace(template, "");

            var host = new RazorEngineHost(new CSharpRazorCodeLanguage(), () => parser)
            {
                DefaultBaseClass = BuildTypeName(baseType, typeof (T)),
                DefaultClassName = key,
                DefaultNamespace = "RazorSharp.Dynamic",
                GeneratedClassContext =
                    new GeneratedClassContext("Execute", "Write", "WriteLiteral", "WriteTo", "WriteLiteralTo",
                        "RazorSharp.TemplateBase")
            };

            //always include this one
            host.NamespaceImports.Add("RazorSharp");
            host.NamespaceImports.Add("System");

            RazorSharpConfigurationSection config = RazorSharpConfigurationSection.GetConfiguration();
            if (config.Namespaces != null && config.Namespaces.Count > 0)
            {
                foreach (NamespaceConfigurationElement @namespace in config.Namespaces)
                {
                    host.NamespaceImports.Add(@namespace.Namespace);
                }
            }

            CodeCompileUnit code;
            using (var reader = new StringReader(template))
            {
                GeneratorResults generatedCode = new RazorTemplateEngine(host).GenerateCode(reader);
                code = generatedCode.GeneratedCode;
            }

            var @params = new CompilerParameters
            {
                IncludeDebugInformation = false,
                TempFiles = new TempFileCollection(AppDomain.CurrentDomain.DynamicDirectory),
                CompilerOptions = "/target:library /optimize",
                GenerateInMemory = false
            };

            string[] assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray();

            @params.ReferencedAssemblies.AddRange(assemblies);

            var provider = new CSharpCodeProvider();
            CompilerResults compiled = provider.CompileAssemblyFromDom(@params, code);

            if (compiled.Errors.Count > 0)
            {
                var errorBuilder = new StringBuilder();
                errorBuilder.AppendLine("Failed to compile");

                var hasErrors = false;

                for (var i = 0; i < compiled.Errors.Count; i++)
                {
                    var error = compiled.Errors[i];
                    hasErrors |= !error.IsWarning;

                    errorBuilder.AppendFormat(
                        "    {0} {1} in {2}:line {3}, col {4}{5}",
                        
                        error.ErrorNumber,
                        error.ErrorText,
                        error.FileName,
                        error.Line,
                        error.Column,
                        Environment.NewLine
                    );
                }

                if (hasErrors)
                {
                    throw new ApplicationException(errorBuilder.ToString());
                }
            }

            return compiled.CompiledAssembly.GetType("RazorSharp.Dynamic." + key);
        }

        internal static bool IsAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (type.IsClass
                    && type.IsSealed
                    && type.BaseType == typeof (object)
                    && type.Name.StartsWith("<>")
                    && type.IsDefined(typeof (CompilerGeneratedAttribute), true));
        }

        internal static bool IsDynamicType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (typeof (DynamicObject).IsAssignableFrom(type)
                    || typeof (ExpandoObject).IsAssignableFrom(type)
                    || IsAnonymousType(type));
        }

        internal static string BuildTypeNameInternal(Type type, bool isDynamic)
        {
            if (!type.IsGenericType)
                return type.FullName;

            return type.Namespace
                   + "."
                   + type.Name.Substring(0, type.Name.IndexOf('`'))
                   + "<"
                   +
                   (isDynamic
                       ? "dynamic"
                       : string.Join(", ",
                           type.GetGenericArguments().Select(t => BuildTypeNameInternal(t, IsDynamicType(t)))))
                   + ">";
        }

        internal static string BuildTypeName(Type templateType, Type modelType)
        {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            if (!templateType.IsGenericTypeDefinition && !templateType.IsGenericType)
                return templateType.FullName;

            if (modelType == null)
                throw new ArgumentException(
                    "The template type is a generic defintion, and no model type has been supplied.");

            bool @dynamic = IsDynamicType(modelType);
            Type genericType = templateType.MakeGenericType(IsDynamicType(modelType) ? typeof (object) : modelType);

            return BuildTypeNameInternal(genericType, @dynamic);
        }
    }
}
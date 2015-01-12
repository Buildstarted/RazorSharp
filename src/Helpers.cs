namespace RazorSharp
{
    using System;
    using System.Linq;

    internal class Helpers
    {
        /// <summary>
        /// Gets the name of the given type, suitable for writing out into C# code.
        /// Fails when there are generic arguments, and the type is not Nullable&lt;&gt;
        /// </summary>
        /// <param name="type">The type to get the name of</param>
        /// <returns>The name of <paramref name="type"/></returns>
        public static string TypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var tmp = type.GetGenericTypeDefinition().Name;

            if (tmp == "Nullable`1")
            {
                return TypeName(type.GetGenericArguments()[0]) + "?";
            }

            tmp = tmp.Substring(0, tmp.IndexOf('`'));
            tmp += String.Format(
                "<{0}>",
                String.Join(
                    ", ",
                    type.GetGenericArguments().Select(TypeName).ToArray()
                )
            );

            return tmp;
        }
    }
}

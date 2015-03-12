namespace Converters.Extentions
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;

    using StyleCop;
    using StyleCop.CSharp;

    public static class NamespaceNodeExtentions
    {
        public static IEnumerable<ICodeElement> Get(this FullyQualifiedNamesCache.NamespaceNode namespaceNode)
        {
            if (namespaceNode != null)
            {
                yield return namespaceNode.CodeElement;
            }
        }

        public static string GetRootedName(this string fullyQualifiedName)
        {
            return string.Concat("Root.", fullyQualifiedName);
        }
    }
}

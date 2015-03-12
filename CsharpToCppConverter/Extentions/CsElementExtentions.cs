namespace Converters.Extentions
{
    using System.CodeDom.Compiler;
    using System.Linq;
    using StyleCop.CSharp;

    public static class CsElementExtentions
    {
        public static bool IsStatic(this CsElement csElement)
        {
            if (csElement == null)
            {
                return false;
            }

            return csElement.ElementTokens.Any(x => x.CsTokenType == CsTokenType.Static);
        }

        public static bool HasToken(this ICodeUnit codeUnit, CsTokenType tokenType)
        {
            if (codeUnit == null)
            {
                return false;
            }

            return codeUnit.Tokens.Any(x => x.CsTokenType == tokenType);
        }

        /// <summary>
        /// The save declatations after modifiers.
        /// </summary>
        /// <param name="csharpElement">
        /// The csharp element.
        /// </param>
        public static void SaveDeclatationsAfterModifiers(this CsElement csharpElement, IndentedTextWriter writer)
        {
            if (csharpElement.Declaration.ContainsModifier(CsTokenType.Static))
            {
                writer.Write("static ");
            }

            // we need to save modifiers
            if (csharpElement.Declaration.ContainsModifier(CsTokenType.Virtual, CsTokenType.Override))
            {
                writer.Write("virtual ");
            }

            if (csharpElement.Declaration.ContainsModifier(CsTokenType.Abstract))
            {
                // destHeader.Write("abstract ");
                writer.Write("virtual ");
            }
        }

    }
}

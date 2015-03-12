namespace Converters
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using StyleCop.CSharp;
    using System.Diagnostics;

    public class SharpToCppConverterHelper
    {
        public static string GetTypeKeyword(CsElement typeDeclaration)
        {
            if (typeDeclaration is StyleCop.CSharp.Class)
            {
                return "class";
            }
            else if (typeDeclaration is StyleCop.CSharp.Struct)
            {
                return "struct";
            }
            else if (typeDeclaration is StyleCop.CSharp.Enum)
            {
                return "enum";
            }
            else if (typeDeclaration is StyleCop.CSharp.Delegate)
            {
                //return "/*delegate*/ typedef void";
                return "/*delegate*/ class";
            }
            else if (typeDeclaration is StyleCop.CSharp.Interface)
            {
                return "/*interface*/ class";
            }

            //// Debug.Assert(false);

            return String.Empty;
        }

        public static void WriteArrayPointerIndexes(Expression expression, IndentedTextWriter writer, string typeReferenceString, int pos)
        {
            // Array converter
            // we converting [,,...,] -> [][]....[]
            string typeIndexes = typeReferenceString.Substring(pos, typeReferenceString.Length - pos);

            List<string> indexes = new List<string>();

            SharpToCppConverterHelper.GetIndexes(expression, indexes, true);

            writer.Write(String.Join("*", indexes.ToArray()));
        }

        public static void WriteArrayIndexes(Expression expression, IndentedTextWriter writer, string typeReferenceString, int pos, bool fieldDeclaration)
        {
            // Array converter
            // we converting [,,...,] -> [][]....[]
            string typeIndexes = typeReferenceString.Substring(pos, typeReferenceString.Length - pos);

            List<string> indexes = new List<string>();

            SharpToCppConverterHelper.GetIndexes(expression, indexes);

            if (indexes.Count == 0)
            {
                return;
            }

            writer.Write(String.Concat(fieldDeclaration && indexes.Count == 0 ? "[0" : "[", String.Join("][", indexes.ToArray()), "]"));
        }

        public static void GetIndexes(Expression expression, IList<string> indexes, bool empty = false)
        {
            ArrayInitializerExpression arrayInitializerExpression = expression as ArrayInitializerExpression;
            if (arrayInitializerExpression != null)
            {
                indexes.Add(empty ? String.Empty : arrayInitializerExpression.Initializers.Count.ToString());
                if (arrayInitializerExpression.Initializers.Count > 0)
                {
                    SharpToCppConverterHelper.GetIndexes(
                        arrayInitializerExpression.Initializers.ElementAt<Expression>(0),
                        indexes);
                }
            }
        }

        public static int GetGenericsTypeNumber(string typeName)
        {
            int level = 0;

            int position = typeName.IndexOf('<');
            if (position < 0)
            {
                return level;
            }

            level = 1;
            int deep = 0;
            do
            {
                char current = typeName[position];

                if (current == '<')
                {
                    deep++;
                }

                if (current == ',' && deep == 1)
                {
                    level++;
                }

                if (current == '>')
                {
                    deep--;
                }
            }
            while (++position < typeName.Length);

            return level;
        }

        public static IEnumerable<string> GetGenericsTypes(string typeName)
        {
            int level = 0;

            int position = typeName.IndexOf('<');
            if (position < 0)
            {
                yield break;
            }

            int startType = 0;

            level = 1;
            int deep = 0;
            do
            {
                char current = typeName[position];

                if (current == '<')
                {
                    deep++;

                    if (deep == 1)
                    {
                        startType = position + 1;
                    }
                }

                if (current == ',' && deep == 1)
                {
                    if (startType == position)
                    {
                        yield break;
                    }

                    yield return typeName.Substring(startType, position - startType);
                    level++;
                }

                if (current == '>')
                {
                    if (deep == 1)
                    {
                        if (startType == position)
                        {
                            yield break;
                        }

                        yield return typeName.Substring(startType, position - startType);
                    }

                    deep--;
                }
            }
            while (++position < typeName.Length);

            yield break;
        }
    }
}

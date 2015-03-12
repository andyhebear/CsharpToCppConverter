namespace Converters.Extentions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StyleCop;
    using StyleCop.CSharp;

    public static class CodePart
    {
        public static T FindParent<T>(this ICodePart codeUnit) where T : class
        {
            var currentElement = codeUnit;
            while (currentElement != null && currentElement.Parent != null)
            {
                if (currentElement.GetType().GUID.Equals(typeof(T).GUID))
                {
                    return currentElement as T;
                }

                currentElement = currentElement.Parent;
            }

            throw new KeyNotFoundException(typeof(T).Name);
        }

        public static T FindParentOfType<T>(this ICodePart codeUnit) where T : class
        {
            var element = FindParentOfTypeOrDefault<T>(codeUnit);
            if (element != null)
            {
                return element;
            }

            throw new KeyNotFoundException(typeof(T).Name);
        }

        public static T FindParentOfTypeOrDefault<T>(this ICodePart codeUnit) where T : class
        {
            var currentElement = codeUnit;
            while (currentElement != null && currentElement.Parent != null)
            {
                var t = currentElement as T;
                if (t != null)
                {
                    return t;
                }

                currentElement = currentElement.Parent;
            }

            return null;
        }

        public static IEnumerable<T> FindAllParentOfTypeOrDefault<T>(this ICodePart codeUnit) where T : class
        {
            var currentElement = codeUnit;
            while (currentElement != null && currentElement.Parent != null)
            {
                var t = currentElement as T;
                if (t != null)
                {
                    yield return t;
                }

                currentElement = currentElement.Parent;
            }
        }

        public static IEnumerable<ICodePart> FindVarDeclarations(this Statement childStatement)
        {
            var variableDecl = childStatement as VariableDeclarationStatement;
            if (variableDecl != null)
            {
                foreach (var declarator in variableDecl.Declarators)
                {
                    yield return declarator;
                }

                yield break;
            }

            var foreachDecl = childStatement as ForeachStatement;
            if (foreachDecl != null)
            {
                foreach (var declarator in foreachDecl.Variable.Declarators)
                {
                    yield return declarator;
                }

                yield break;
            }

            var forDecl = childStatement as ForStatement;
            if (forDecl != null)
            {
                foreach (var variableDeclFor in forDecl.Initializers.OfType<VariableDeclarationExpression>())
                {
                    foreach (var declarator in variableDeclFor.Declarators)
                    {
                        yield return declarator;
                    }
                }

                yield break;
            }
        }

        public static string GetNameFromCodeElement(this ICodeElement member)
        {
            if (member == null)
            {
                return string.Empty;
            }

            var fullyQualifiedName = member.FullyQualifiedName;
            var parametersCharPosition = member.FullyQualifiedName.IndexOf('%');
            if (parametersCharPosition != -1)
            {
                fullyQualifiedName = fullyQualifiedName.Substring(0, parametersCharPosition);
            }

            return fullyQualifiedName.Split('.').Last();
        }

        public static string GetNameFromCodePart(this ICodePart codePart)
        {
            var parameter = codePart as Parameter;
            if (parameter != null)
            {
                return parameter.Name;
            }

            var variableDeclaratorExpression = codePart as VariableDeclaratorExpression;
            if (variableDeclaratorExpression != null)
            {
                return variableDeclaratorExpression.Identifier.Text;
            }

            throw new NotImplementedException();
        }

        public static bool MemberNameEqualsTo(this ICodeElement codeElement, string name)
        {
            return string.CompareOrdinal(name, GetNameFromCodeElement(codeElement)) == 0;
        }
    }
}

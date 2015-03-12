// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeResolver.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the TypeResolver type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Converters
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using Converters.Dictionaries;
    using Converters.Enums;
    using Converters.Extentions;
    using Converters.Metadata;

    using StyleCop;
    using StyleCop.CSharp;

    // todo: redesign class completelly
    // todo: use GenericType
    public class TypeResolver
    {
        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICodeElement codeElement;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isArray;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isAuto;

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private bool isMethodParameterName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isPointer;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isReference;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isTemplate;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private INamesResolver namesResolver;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string @namespace;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string type;

        #endregion

        #region Constructors and Destructors

        public TypeResolver(GenericType typeTokenReference, INamesResolver namesInfoHolder)
            : this(typeTokenReference.Text, namesInfoHolder)
        {
        }

        public TypeResolver(TypeToken typeTokenReference, INamesResolver namesInfoHolder)
            : this(typeTokenReference.Text, namesInfoHolder)
        {
        }

        [Obsolete("use token type of instead")]
        public TypeResolver(string type, INamesResolver namesInfoHolder)
        {
            this.SetProperties(type, namesInfoHolder);
            this.CalculateProperties();
        }

        #endregion

        #region Public Properties

        public ICodeElement CodeElement
        {
            get
            {
                return this.codeElement;
            }
        }

        public string FullyQualifiedName
        {
            get
            {
                // Debug.Assert(this.CodeElement != null, "CodeElement != null");
                if (this.CodeElement == null)
                {
                    return string.Empty;
                }

                return this.CodeElement.FullyQualifiedName.Substring("Root.".Length);
            }
        }

        public bool HasCodeElement
        {
            get
            {
                return this.CodeElement != null;
            }
        }

        public bool IsClassName
        {
            get
            {
                var metadataAdapter = this.CodeElement as MetadataICodeElementAdapter;
                if (metadataAdapter != null)
                {
                    return metadataAdapter.IsClassName;
                }

                return this.CodeElement is ClassBase;
            }
        }

        public bool IsEnum
        {
            get
            {
                return this.CodeElement is StyleCop.CSharp.Enum;
            }
        }

        public bool IsFieldOrVariable
        {
            get
            {
                var metadataAdapter = this.CodeElement as MetadataICodeElementAdapter;
                if (metadataAdapter != null)
                {
                    return metadataAdapter.IsFieldOrVariable;
                }

                return this.CodeElement is Field;
            }
        }

        public bool IsNamespace
        {
            get
            {
                var metadataAdapter = this.CodeElement as MetadataICodeElementAdapter;
                if (metadataAdapter != null)
                {
                    return metadataAdapter.IsNamespace;
                }

                return this.CodeElement is Namespace;
            }
        }

        public bool IsNamespaceInUsingDerictives
        {
            get
            {
                return this.namesResolver.UsingDirectives.Contains(this.Namespace);
            }
        }

        public bool IsReference
        {
            get
            {
                return this.isReference;
            }
        }

        public bool IsResolved
        {
            get
            {
                return this.IsNamespace || this.IsClassName || this.IsEnum || this.IsStatic;
            }
        }

        public bool IsStatic
        {
            get
            {
                if (!this.HasCodeElement)
                {
                    return false;
                }

                var metadataAdapter = this.CodeElement as MetadataICodeElementAdapter;
                if (metadataAdapter != null)
                {
                    return metadataAdapter.IsStatic;
                }

                var element = this.CodeElement as CsElement;
                if (element != null)
                {
                    return element.IsStatic();
                }

                return false;
            }
        }

        public string Namespace
        {
            get
            {
                return this.@namespace;
            }
        }

        public FullyQualifiedNamesCache.NamespaceNode NamespaceNode { get; private set; }

        #endregion

        #region Public Methods and Operators

        public string GetCXFullyQualifiedName(SavingOptions savingOptions)
        {
            var name = this.GetCxName(savingOptions);

            if (!string.IsNullOrEmpty(this.Namespace))
            {
                var lastNamespaceChars = name.Split('<')[0].LastIndexOf("::");
                if (lastNamespaceChars != -1)
                {
                    name = name.Substring(lastNamespaceChars + 2);
                }

                return string.Concat(this.Namespace.Replace(".", "::"), "::", name);
            }

            return name;
        }

        public string GetCxFullyQualifiedType(SavingOptions savingOptions)
        {
            return this.BuildType(savingOptions, this.GetCXFullyQualifiedName(savingOptions));
        }

        public string GetCxName(SavingOptions savingOptions)
        {
            if (this.isAuto)
            {
                return "auto";
            }

            if (!this.IsReference)
            {
                return this.type;
            }

            if (!this.isTemplate)
            {
                return ConvertNamespaceChars(this.type);
            }

            var sb = new StringBuilder(100);

            // you need to convert all template types into correct types
            sb.Append(this.GetCxTypeNameWithoutTemplateParameters());

            sb.Append('<');

            var first = true;
            foreach (var templateType in CXXConverterLogic.GetGenericsTypes(this.type))
            {
                var referencedType = new TypeResolver(templateType, this.namesResolver);

                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append(
                    savingOptions.HasFlag(SavingOptions.UseFullyQualifiedNames)
                        ? referencedType.GetCxFullyQualifiedType(SavingOptions.None)
                        : referencedType.GetCxType(SavingOptions.None));
            }

            sb.Append('>');

            return sb.ToString();
        }

        public string GetCxType(SavingOptions savingOptions)
        {
            return this.BuildType(savingOptions, this.GetCxName(savingOptions));
        }

        #endregion

        #region Methods

        private static string ConvertNamespaceChars(string typeToken)
        {
            return typeToken.Replace(".", "::");
        }

        private string BuildType(SavingOptions savingOptions, string name)
        {
            var buildType = name;

            if ((this.NamespaceNode != null && !savingOptions.HasFlag(SavingOptions.RemovePointer) && this.isReference) || this.isPointer)
            {
                buildType = string.Concat(buildType, "^");
            }

            if (savingOptions.HasFlag(SavingOptions.ApplyReference))
            {
                buildType = string.Concat(buildType, "&");
            }

            if (savingOptions.HasFlag(SavingOptions.ApplyRvalueReference))
            {
                buildType = string.Concat(buildType, "&&");
            }

            return buildType;
        }

        // todo: on Dictionary<string, object> it didn't return Namespace node, find out why
        // because there is no namespace System
        private void CalculateProperties()
        {
            var fullName = this.GetFullyQualifiedNameForBuiltInTypes(this.namesResolver.IsPlatformUsedInUsingDirectives);

            //this.isMethodParameterName = false;
            //if (this.namesResolver.CurrentParameters != null)
            //{
            //    this.isMethodParameterName = this.namesResolver.CurrentParameters.Any(p => p.Name == this.type);
            //}

            //if (!this.isMethodParameterName)
            //{
                this.codeElement = null;
                this.@namespace = null;

                var namespaceTypeName = this.GetNamespaceTypeName(fullName);
                var mappedValue = this.GetMappedTypeName(namespaceTypeName);

                this.NamespaceNode = this.namesResolver.FindNamespaceNode(mappedValue);
                if (this.NamespaceNode != null)
                {
                    this.codeElement = this.NamespaceNode.CodeElement;
                    this.@namespace = this.NamespaceNode.FullNamespace;
                }
            //}

            if (this.NamespaceNode != null)
            {
                this.type = this.GetTypeNameOnly(this.type);
            }

            // Debug.Assert(found, "Can't find type");
            this.ConvertCSharpTypeToCxTypeAndSetIsReference(this.namesResolver.IsPlatformUsedInUsingDirectives);
        }

        private void ConvertCSharpTypeToCxTypeAndSetIsReference(bool isPlatfromIncluded)
        {
            switch (this.type)
            {
                case "void":
                case "bool":
                case "double":
                case "float":
                case "int":
                case "long":
                case "short":
                    break;
                case "char":
                    this.type = "wchar_t";
                    break;
                case "sbyte":
                    this.type = "char";
                    break;
                case "byte":
                    this.type = "unsigned char";
                    break;
                case "uint":
                case "ulong":
                case "ushort":
                    this.type = string.Concat("unsigned ", this.type.Substring(1));
                    break;
                case "decimal":
                    this.isReference = true;
                    this.type = isPlatfromIncluded ? "Decimal" : "Platform::Decimal";
                    break;
                case "String":
                case "string":
                    this.isReference = true;
                    this.type = isPlatfromIncluded ? "String" : "Platform::String";
                    break;
                case "Object":
                case "object":
                    this.isReference = true;
                    this.type = isPlatfromIncluded ? "Object" : "Platform::Object";
                    break;

                default:

                    // check if found codeElement is TypeDef for value
                    var metadataCode = this.codeElement as MetadataICodeElementAdapter;
                    this.isReference = !this.isAuto && !(metadataCode != null && metadataCode.IsValueType);
                    break;
            }
        }

        private string GetCxTypeNameWithoutTemplateParameters()
        {
            return ConvertNamespaceChars(this.GetTypeNameOrGenericBaseType());
        }

        [Obsolete]
        private string GetFullyQualifiedNameForBuiltInTypes(bool platformIncluded)
        {
            string foundFullyQualifiedName;
            if (FullyQualifiedNames.Map.TryGetValue(this.type, out foundFullyQualifiedName))
            {
                return platformIncluded ? this.GetTypeNameOnly(foundFullyQualifiedName) : foundFullyQualifiedName;
            }

            return this.type;
        }

        private string GetMappedTypeName(string namespaceTypeName)
        {
            string mappedType;
            if (CsTypesToCppTypes.Map.TryGetValue(namespaceTypeName, out mappedType))
            {
                return mappedType;
            }

            return namespaceTypeName;
        }

        private string GetNamespaceTypeName(string typeToken)
        {
            var pos = typeToken.IndexOf('<');
            if (pos == -1)
            {
                return typeToken;
            }

            var baseType = typeToken.Substring(0, pos);

            var templatesCount = 1;
            var deepLevel = 0;
            while (pos < typeToken.Length)
            {
                var c = typeToken[pos++];
                if (c == '<')
                {
                    deepLevel++;
                }

                if (deepLevel == 1 && c == ',')
                {
                    templatesCount++;
                }

                if (c == '>')
                {
                    deepLevel--;
                }
            }

            return string.Concat(baseType, '`', templatesCount);
        }

        private string GetTypeNameOnly(string fullyQualifiedName)
        {
            var typeNameOnly = this.GetTypeNameOrGenericBaseType(fullyQualifiedName);
            var pos = typeNameOnly.LastIndexOf('.');
            if (pos >= 0)
            {
                return fullyQualifiedName.Substring(pos + 1);
            }

            return fullyQualifiedName;
        }

        private string GetTypeNameOrGenericBaseType()
        {
            return this.GetTypeNameOrGenericBaseType(this.type);
        }

        private string GetTypeNameOrGenericBaseType(string typeToken)
        {
            var pos = typeToken.IndexOf('<');
            if (pos == -1)
            {
                return typeToken;
            }

            return typeToken.Substring(0, pos);
        }

        private void SetProperties(string type, INamesResolver namesResolver)
        {
            this.namesResolver = namesResolver;

            this.isReference = false;
            this.isPointer = type.LastIndexOf('*') != -1;
            this.isTemplate = type.LastIndexOf('<') != -1 && type.LastIndexOf('>') != -1;
            this.isArray = type.LastIndexOf('[') != -1;
            this.isAuto = "var".Equals(type);

            var parts = type.Split('[', ']', '*' /*, '`'*/);
            this.type = parts[0];
        }

        #endregion
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Mr O. Duzhar" file="SharpToCppInterpreter.cs">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   The Sharp To Cpp Interpreter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Converters
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Converters.Adapters;
    using Converters.Enums;
    using Converters.Extentions;

    using StyleCop;
    using StyleCop.CSharp;

    using Delegate = StyleCop.CSharp.Delegate;
    using Enum = StyleCop.CSharp.Enum;
    using Attribute = StyleCop.CSharp.Attribute;

    /// <summary>
    /// The cs document saver.
    /// </summary>
    public class SharpToCppInterpreter : INamesResolver
    {
        #region Fields

        private readonly CsParser defaultParser;

        /// <summary>
        ///   The document.
        /// </summary>
        private readonly CsDocument document;

        private readonly CodeProject emptyCodeProject;

        /// <summary>
        ///   The name.
        /// </summary>
        private readonly string name;

        /// <summary>
        ///   The stack current class.
        /// </summary>
        private readonly IList<ClassContext> stackCurrentClass = new List<ClassContext>();

        /// <summary>
        ///   The stack current parameters.
        /// </summary>
        private readonly IList<IList<Parameter>> stackCurrentParameters = new List<IList<Parameter>>();

        /// <summary>
        ///   The stream stack.
        /// </summary>
        private readonly Stack<KeyValuePair<IndentedTextWriter, IndentedTextWriter>> streamStack =
            new Stack<KeyValuePair<IndentedTextWriter, IndentedTextWriter>>();

        /// <summary>
        ///   The using directives.
        /// </summary>
        private readonly IList<string> usingDirectives = new List<string>();

        /// <summary>
        ///   The dest cpp.
        /// </summary>
        private IndentedTextWriter cppWriter;

        /// <summary>
        ///   The current base class.
        /// </summary>
        private string currentBaseClass;

        /// <summary>
        ///   The current class namespace.
        /// </summary>
        private string currentClassNamespace;

        /// <summary>
        ///   The current namespace.
        /// </summary>
        private Namespace currentNamespace;

        /// <summary>
        ///   The current namespace last name.
        /// </summary>
        private string currentNamespaceLastName;

        /// <summary>
        ///   The dest header.
        /// </summary>
        private IndentedTextWriter headerWriter;

        /// <summary>
        ///   The is cpp in header.
        /// </summary>
        private bool isCppInHeader;

        /// <summary>
        ///   The last block statement.
        /// </summary>
        private bool lastBlockStatement;

        /// <summary>
        ///   The last column.
        /// </summary>
        private bool lastColumn;

        /// <summary>
        ///   The letteral others prefix.
        /// </summary>
        private string letteralOthersPrefix;

        /// <summary>
        ///   The current using namespaces root.
        /// </summary>
        private IList<FullyQualifiedNamesCache.NamespaceNode> resolvedUsingDirectives;

        /// <summary>
        ///   The save variables mode.
        /// </summary>
        // todo: get rid of it
        private SaveVariablesMode saveVariablesMode;

        /// <summary>
        ///   The saved dest cpp.
        /// </summary>
        private IndentedTextWriter savedDestCpp;

        // private ClassContext currentClass;

        /// <summary>
        ///   The start of statement.
        /// </summary>
        private bool startOfStatement;

        #endregion

        // Save variables mode
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SharpToCppInterpreter"/> class. 
        /// Initializes a new instance of the <see cref="CsDocumentSaver"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public SharpToCppInterpreter(CsDocument document)
        {
            this.document = document;
            this.name = this.Document.SourceCode.Name.Substring(0, this.Document.SourceCode.Name.Length - 3);
            this.emptyCodeProject = new CodeProject(
                string.Empty.GetHashCode(), string.Empty, new Configuration(new string[] { }));
            this.defaultParser = new CsParser();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets ClassContext.
        /// </summary>
        public ClassContext ClassContext
        {
            get
            {
                if (this.stackCurrentClass.Count > 0)
                {
                    return this.stackCurrentClass[this.stackCurrentClass.Count - 1];
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    if (this.stackCurrentClass.Count > 0)
                    {
                        this.stackCurrentClass.RemoveAt(this.stackCurrentClass.Count - 1);
                    }
                }
                else
                {
                    this.stackCurrentClass.Add(value);
                }
            }
        }

        /// <summary>
        ///   Gets or sets CurrentNamespace.
        /// </summary>
        public Namespace CurrentNamespace
        {
            get
            {
                return this.currentNamespace;
            }

            protected set
            {
                this.currentNamespace = value;

                var parts = this.currentNamespace.Declaration.Name.Split('.');
                this.currentNamespaceLastName = parts[parts.Length - 1];

                this.CppLangCurrentFullNamespaceName = this.CurrentNamespaceName.Replace(".", "::");
            }
        }

        /// <summary>
        ///   Gets or sets CurrentParameters.
        /// </summary>
        public IList<Parameter> CurrentParameters
        {
            get
            {
                if (this.stackCurrentParameters.Count > 0)
                {
                    return this.stackCurrentParameters[this.stackCurrentParameters.Count - 1];
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    if (this.stackCurrentParameters.Count > 0)
                    {
                        this.stackCurrentParameters.RemoveAt(this.stackCurrentParameters.Count - 1);
                    }
                }
                else
                {
                    this.stackCurrentParameters.Add(value);
                }
            }
        }

        /// <summary>
        ///   The document.
        /// </summary>
        public CsDocument Document
        {
            get
            {
                return this.document;
            }
        }

        /// <summary>
        ///   Gets or sets FullyQualifiedNames.
        /// </summary>
        /// 
        public FullyQualifiedNamesCache FullyQualifiedNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is platform used in using directives.
        /// </summary>
        public bool IsPlatformUsedInUsingDirectives { get; protected set; }

        /// <summary>
        ///   Gets or sets OutputDestinationFolder.
        /// </summary>
        public string OutputDestinationFolder { get; set; }

        /// <summary>
        ///   Gets UsingDirectives.
        /// </summary>
        public IList<string> UsingDirectives
        {
            get
            {
                return this.usingDirectives;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets CPPCurrentFullNamespaceName.
        /// </summary>
        protected string CppLangCurrentFullNamespaceName { get; private set; }

        /// <summary>
        ///   Gets CurrentFullNamespaceName.
        /// </summary>
        protected string CurrentFullNamespaceName
        {
            get
            {
                if (this.currentNamespace == null)
                {
                    return "__anonymous";
                }

                return this.currentNamespace.FullNamespaceName.Substring("Root.".Length);
            }
        }

        /// <summary>
        ///   Gets CurrentNamespaceLastName.
        /// </summary>
        protected string CurrentNamespaceLastName
        {
            get
            {
                return this.currentNamespaceLastName;
            }
        }

        /// <summary>
        ///   Gets CurrentNamespaceName.
        /// </summary>
        protected string CurrentNamespaceName
        {
            get
            {
                return this.currentNamespace.Declaration.Name;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether IsCPPInHeader.
        /// </summary>
        protected bool IsCPPInHeader
        {
            get
            {
                return this.isCppInHeader;
            }

            set
            {
                if (this.isCppInHeader == value)
                {
                    return;
                }

                this.isCppInHeader = value;

                if (this.isCppInHeader)
                {
                    this.savedDestCpp = this.cppWriter;
                    this.cppWriter = this.headerWriter;
                }
                else if (this.savedDestCpp != null)
                {
                    // restore destCPP;
                    this.cppWriter = this.savedDestCpp;
                    this.savedDestCpp = null;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The find code element.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="NamespaceNode"/>.
        /// </returns>
        public FullyQualifiedNamesCache.NamespaceNode FindNamespaceNode(string name)
        {
            FullyQualifiedNamesCache.NamespaceNode codeElement = null;
            var currentClass = this.ClassContext;
            if (currentClass != null)
            {
                codeElement = currentClass[name];
                if (codeElement != null)
                {
                    return codeElement;
                }
            }

            var namespaceNode = this.FullyQualifiedNames[name];
            if (namespaceNode != null)
            {
                return namespaceNode;
            }

            if (this.resolvedUsingDirectives == null)
            {
                this.LoadNamespaceNodes();
            }

            return this.resolvedUsingDirectives == null
                       ? null
                       : this.resolvedUsingDirectives.Where(d => d != null).Select(d => d[name]).FirstOrDefault(n => n != null);

            ////Debug.Assert(false);
        }

        /// <summary>
        /// The get current class namespace name.
        /// </summary>
        /// <returns>
        /// The get current class namespace name.
        /// </returns>
        public string GetCurrentClassNamespaceName()
        {
            if (this.ClassContext != null)
            {
                return this.ClassContext.Class.FullNamespaceName.Substring("Root.".Length);
            }

            return string.Empty;
        }

        /// <summary>
        /// The get current namespace name.
        /// </summary>
        /// <returns>
        /// The get current namespace name.
        /// </returns>
        public string GetCurrentNamespaceName()
        {
            return this.currentNamespace.Name.Substring("namespace ".Length);
        }

        [Obsolete("seems it is us")]
        public TypeToken ParseType(string type)
        {
            // create dummy wraper
            var codeText = string.Concat("class M { ", type, " x; }");

            var code = new CodeText(codeText, this.emptyCodeProject, this.defaultParser);

            CodeDocument codeDocument = null;

            code.Parser.PreParse();
            try
            {
                var requiredNextPass = code.Parser.ParseFile(code, 0, ref codeDocument);
            }
            catch (ArgumentException)
            {
                return null;
            }
            finally
            {
                code.Parser.PostParse();
            }

            var classDecl = codeDocument.DocumentContents.ChildCodeElements.First();
            var fieldDecl = classDecl.ChildCodeElements.First() as Field;
            return fieldDecl.FieldType;
        }

        /// <summary>
        /// The save.
        /// </summary>
        public void Save()
        {
            if (!string.IsNullOrEmpty(this.OutputDestinationFolder) && !Directory.Exists(this.OutputDestinationFolder))
            {
                Directory.CreateDirectory(this.OutputDestinationFolder);
            }

            this.PushStreams();
            this.headerWriter =
                new IndentedTextWriter(
                    new StreamWriter(Path.Combine(this.OutputDestinationFolder, string.Concat(this.name, ".h"))));
            this.cppWriter =
                new IndentedTextWriter(
                    new StreamWriter(Path.Combine(this.OutputDestinationFolder, string.Concat(this.name, ".cpp"))));

            // save common header
            this.WriteHeaderProlog();
            this.WriteCppProlog();

            // save all preprocessor command to include header files
            var csharpDocument = this.Document as CsDocument;
            foreach (var token in csharpDocument.Tokens)
            {
                if (token.CsTokenType == CsTokenType.PreprocessorDirective)
                {
                    var preprocessor = token as Preprocessor;
                    if (preprocessor != null && preprocessor.PreprocessorType.Equals("pragma"))
                    {
                        var cppInclude =
                            preprocessor.Text.SubstringAfter("pragma", StringComparison.Ordinal).TrimStart();
                        if (cppInclude.StartsWith("include"))
                        {
                            this.headerWriter.Write("#");
                            this.headerWriter.WriteLine(cppInclude);
                        }
                    }
                }
            }

            this.Save(this.Document.DocumentContents.ChildCodeElements);

            this.headerWriter.WriteLine();
            this.cppWriter.WriteLine();

            this.headerWriter.Close();
            this.cppWriter.Close();
            this.PopStreams();
        }

        /// <summary>
        /// The write cpp prolog.
        /// </summary>
        public void WriteCppProlog()
        {
            this.cppWriter.WriteLine("#include \"pch.h\"");
            this.cppWriter.WriteLine("#include \"{0}\"", string.Concat(this.name, ".h"));
            this.cppWriter.WriteLine();
        }

        /// <summary>
        /// The write header prolog.
        /// </summary>
        public void WriteHeaderProlog()
        {
            // this.headerWriter.WriteLine("#include \"pch.h\"");
            this.headerWriter.WriteLine("#pragma once");
            this.headerWriter.WriteLine();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add ends for namespace.
        /// </summary>
        private void AddEndsForNamespace()
        {
            var declarationName = this.CurrentNamespaceLastName;
            var @fullNamespace = this.CurrentFullNamespaceName;

            // Add Namespaces
            this.AddNamespaceOperatorsEnd(@fullNamespace, this.headerWriter);
        }

        /// <summary>
        /// The add header for namespace.
        /// </summary>
        private void AddHeaderForNamespace()
        {
            ////declName = declarationName;
            var declarationName = this.CurrentNamespaceLastName;
            var @fullNamespace = this.CurrentFullNamespaceName;

            // Add Namespaces
            this.AddNamespaceOperatorsBegin(@fullNamespace, this.headerWriter);

            // Add Namespaces
            this.cppWriter.Write("using namespace ");
            this.cppWriter.Write(@fullNamespace.Replace(".", "::"));
            this.cppWriter.WriteLine(";");
            this.cppWriter.WriteLine();
        }

        /// <summary>
        /// The add namespace operators begin.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="itw">
        /// The itw.
        /// </param>
        private void AddNamespaceOperatorsBegin(string namespaceName, IndentedTextWriter itw)
        {
            var namespaceNameLeft = namespaceName;
            var namespaceNameRight = string.Empty;

            var pos = namespaceNameLeft.IndexOf('.');

            if (pos != -1)
            {
                namespaceNameLeft = namespaceName.Substring(0, pos);
                namespaceNameRight = namespaceName.Substring(pos + 1);
            }
            else
            {
            }

            itw.WriteLine("namespace {0}", namespaceNameLeft);
            itw.WriteLine('{');
            itw.Indent++;

            if (string.IsNullOrEmpty(namespaceNameRight))
            {
                // BODY OF DECLARETION
            }
            else
            {
                this.AddNamespaceOperatorsBegin(namespaceNameRight, itw);
            }
        }

        /// <summary>
        /// The add namespace operators end.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="itw">
        /// The itw.
        /// </param>
        private void AddNamespaceOperatorsEnd(string namespaceName, IndentedTextWriter itw)
        {
            var namespaceNameLeft = namespaceName;
            var namespaceNameRight = string.Empty;

            var pos = namespaceNameLeft.IndexOf('.');

            if (pos != -1)
            {
                namespaceNameLeft = namespaceName.Substring(0, pos);
                namespaceNameRight = namespaceName.Substring(pos + 1);
            }
            else
            {
            }

            if (string.IsNullOrEmpty(namespaceNameRight))
            {
                // BODY OF DECLARETION
            }
            else
            {
                this.AddNamespaceOperatorsEnd(namespaceNameRight, itw);
            }

            itw.Indent--;
            itw.WriteLine('}');
        }

        /// <summary>
        /// The add namespace part.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="itw">
        /// The itw.
        /// </param>
        private void AddNamespacePart(string namespaceName, IndentedTextWriter itw)
        {
            var namespaceNameLeft = namespaceName;
            var namespaceNameRight = string.Empty;

            var pos = namespaceNameLeft.IndexOf('.');

            if (pos != -1)
            {
                namespaceNameLeft = namespaceName.Substring(0, pos);
                namespaceNameRight = namespaceName.Substring(pos + 1);
            }
            else
            {
            }

            itw.WriteLine("namespace {0}", namespaceNameLeft);
            itw.WriteLine('{');
            itw.Indent++;

            if (string.IsNullOrEmpty(namespaceNameRight))
            {
                itw.WriteLine("#include \"__predefines.h\"");

                ////itw.WriteLine("#include \"__includes.h\"");
            }
            else
            {
                this.AddNamespacePart(namespaceNameRight, itw);
            }

            itw.Indent--;
            itw.WriteLine('}');
        }

        /// <summary>
        /// The add namespace start.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="itw">
        /// The itw.
        /// </param>
        private void AddNamespaceStart(string namespaceName, IndentedTextWriter itw)
        {
            var nameOfNS = namespaceName.ToUpperInvariant().Replace(".", "_").Replace("`", "$");

            itw.WriteLine("#ifndef __{0}__", nameOfNS);

            // itw.WriteLine("#define __{0}__", nameOfNS);
            itw.WriteLine(
                "#pragma message( \" [[[ Namespace: '{0}' -- Compiling \" __FILE__ \" ]]] \" )", namespaceName);

            ////itw.WriteLine("#pragma message( \" [[[ Compiling \" __FILE__ \" ]]] \" )");
            ////itw.WriteLine("#pragma message( \"Last modified on \" __TIMESTAMP__ )");
            itw.WriteLine("#pragma message(\"\")");

            this.AddNamespacePart(namespaceName, itw);

            ////itw.WriteLine("#include \"__includes.h\"");
            itw.WriteLine("#undef __{0}__", nameOfNS);
            itw.WriteLine("#else");
            itw.WriteLine("#pragma message( \" [[[ NAMESPACE ALREADY DEFINED: {0} ]]] \")", nameOfNS);
            itw.WriteLine("#endif //__{0}__", nameOfNS);
        }

        /// <summary>
        /// The build declaretion template part.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <returns>
        /// The build declaretion template part.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        private string BuildDeclaretionTemplatePart(string typeName)
        {
            var pos = typeName.IndexOf("<");
            if (pos == -1)
            {
                return string.Empty;
            }

            var lastPos = typeName.IndexOf(">");
            if (lastPos == -1)
            {
                throw new Exception("Wrong format");
            }

            var parts = typeName.Substring(pos + 1, lastPos - pos - 1).Split(',');

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("template <");

            foreach (var part in parts)
            {
                if (stringBuilder.Length > "template <".Length)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(" typename ");
                stringBuilder.Append(part);
            }

            stringBuilder.Append(" > ");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// The clear logical expression prefix for others.
        /// </summary>
        private void ClearLogicalExpressionPrefixForOthers()
        {
            this.letteralOthersPrefix = string.Empty;
        }

        /// <summary>
        /// The clear mark begin of non block statement.
        /// </summary>
        private void ClearMarkBeginOfNonBlockStatement()
        {
            this.lastBlockStatement = false;
            this.lastColumn = false;
            this.startOfStatement = false;
        }

        /// <summary>
        /// The get full namespace.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <returns>
        /// The get full namespace.
        /// </returns>
        private string GetFullNamespace(string typeName)
        {
            // write procedure to find full namespace
            return this.CurrentFullNamespaceName;
        }

        /// <summary>
        /// The get function prefix.
        /// </summary>
        /// <param name="accessor">
        /// The accessor.
        /// </param>
        /// <returns>
        /// The get function prefix.
        /// </returns>
        private string GetFunctionPrefix(Accessor accessor)
        {
            // 1 to destHeader
            var functionPrefix = string.Empty;
            switch (accessor.AccessorType)
            {
                case AccessorType.Add:
                    functionPrefix = "add_";
                    break;
                case AccessorType.Remove:
                    functionPrefix = "remove_";
                    break;
                case AccessorType.Get:
                    functionPrefix = "get_";
                    break;
                case AccessorType.Set:
                    functionPrefix = "set_";
                    break;
                default:
                    break;
            }

            return functionPrefix;
        }

        /// <summary>
        /// The get name base.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <returns>
        /// The get name base.
        /// </returns>
        private string GetNameBase(string typeName)
        {
            return this.GetNameBase(typeName, false);
        }

        /// <summary>
        /// The get name base.
        /// </summary>
        /// <param name="typeName">
        /// The type name.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// The get name base.
        /// </returns>
        private string GetNameBase(string typeName, bool template)
        {
            Debug.Assert(typeName != null, "baseName is null");

            var pos = typeName.IndexOf("<");
            if (pos == -1 && !template)
            {
                return typeName;
            }

            return string.Concat('T', template ? typeName : typeName.Substring(0, pos));
        }

        /// <summary>
        /// The has any pointer type.
        /// </summary>
        /// <param name="params">
        /// The params.
        /// </param>
        /// <returns>
        /// The has any pointer type.
        /// </returns>
        private bool HasAnyPointerType(IParameterContainer @params)
        {
            if (@params != null)
            {
                return @params.Parameters.Any(x => x.Type.Text.Contains("*"));
            }

            return false;
        }

        /// <summary>
        /// The is joint statement.
        /// </summary>
        /// <param name="statement">
        /// The statement.
        /// </param>
        /// <returns>
        /// The is joint statement.
        /// </returns>
        private bool IsJointStatement(Statement statement)
        {
            return statement.StatementType == StatementType.Else || statement.StatementType == StatementType.Catch
                   || statement.StatementType == StatementType.Finally;
        }

        /// <summary>
        /// The is separated statement.
        /// </summary>
        /// <param name="statement">
        /// The statement.
        /// </param>
        /// <returns>
        /// The is separated statement.
        /// </returns>
        private bool IsSeparatedStatement(Statement statement)
        {
            return statement.StatementType == StatementType.If || statement.StatementType == StatementType.For
                   || statement.StatementType == StatementType.Foreach || statement.StatementType == StatementType.While;
        }

        /// <summary>
        /// The load namespace nodes.
        /// </summary>
        private void LoadNamespaceNodes()
        {
            // load all nodes
            this.resolvedUsingDirectives = new List<FullyQualifiedNamesCache.NamespaceNode>
                {
                    this.FullyQualifiedNames.FindNamespaceNode(this.CurrentFullNamespaceName) 
                };

            // add current namespace
            foreach (var usingDirective in this.UsingDirectives)
            {
                var namespaceNode = this.FullyQualifiedNames.FindNamespaceNode(usingDirective);

                // Debug.Assert(namespaceNode != null, "not fully ref. namespace");
                if (namespaceNode != null)
                {
                    // resolve namespace
                    this.resolvedUsingDirectives.Add(namespaceNode);
                }
            }
        }

        /// <summary>
        /// The pop streams.
        /// </summary>
        private void PopStreams()
        {
            var popped = this.streamStack.Pop();
            this.headerWriter = popped.Key;
            this.cppWriter = popped.Value;
        }

        /// <summary>
        /// The push streams.
        /// </summary>
        private void PushStreams()
        {
            this.streamStack.Push(
                new KeyValuePair<IndentedTextWriter, IndentedTextWriter>(this.headerWriter, this.cppWriter));
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="childCodeElements">
        /// The child code elements.
        /// </param>
        private void Save(IEnumerable<ICodeElement> childCodeElements)
        {
            foreach (var codeElement in childCodeElements)
            {
                this.@switch(codeElement);
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="namespace">
        /// The namespace.
        /// </param>
        private void Save(Namespace @namespace)
        {
            this.CurrentNamespace = @namespace;

            this.AddHeaderForNamespace();

            this.@switch(@namespace.ChildElements);

            this.AddEndsForNamespace();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="usingDirective">
        /// The using directive.
        /// </param>
        private void Save(UsingDirective usingDirective)
        {
            var realNamespace = usingDirective.NamespaceType;
            if (!string.IsNullOrEmpty(usingDirective.Alias))
            {
                var pos = realNamespace.LastIndexOf('.');
                if (pos >= 0)
                {
                    realNamespace = realNamespace.Substring(0, pos);
                }
            }

            this.usingDirectives.Add(realNamespace);

            if ("Platform".Equals(realNamespace))
            {
                this.IsPlatformUsedInUsingDirectives = true;
            }

            this.cppWriter.Write("using namespace ");
            this.cppWriter.Write(realNamespace.Replace(".", "::"));
            this.cppWriter.WriteLine(";");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="class">
        /// The class.
        /// </param>
        private void Save(ClassBase @class)
        {
            // this is template
            this.IsCPPInHeader = @class.Declaration.Name.Contains('<');

            // save attributes
            foreach (var attr in @class.Attributes)
            {
                this.Save(attr, this.headerWriter);
            }

            var nakedClassName = this.GetNameBase(@class.Declaration.Name);

            this.ClassContext = new ClassContext(@class, this.FullyQualifiedNames);

            if (@class.AccessModifier != AccessModifierType.Internal)
            {
                this.Save(@class.AccessModifier);
                this.headerWriter.Write(" ");
            }

            this.headerWriter.Write("ref ");

            this.headerWriter.Write(this.BuildDeclaretionTemplatePart(@class.Declaration.Name));

            this.currentClassNamespace = nakedClassName;

            var typeKeyword = SharpToCppConverterHelper.GetTypeKeyword(@class);

            this.headerWriter.Write("{1} {0}", nakedClassName, typeKeyword);

            if (@class.HasToken(CsTokenType.Sealed))
            {
                this.headerWriter.Write(" sealed");
            }

            var baseClass = this.GetNameBase(@class.BaseClass, false);

            var hasColon = false;
            if (!string.IsNullOrEmpty(baseClass) && !baseClass.Equals(typeKeyword))
            {
                this.headerWriter.Write(": public ");

                this.currentBaseClass = baseClass;

                this.Save(
                    baseClass, this.headerWriter, SavingOptions.UseFullyQualifiedNames | SavingOptions.RemovePointer);

                hasColon = true;
            }

            // writer interfaces
            if (!hasColon && @class.ImplementedInterfaces != null && @class.ImplementedInterfaces.Count > 0)
            {
                this.headerWriter.Write(": ");
            }

            var first = !hasColon;
            foreach (var interfaceOfClass in @class.ImplementedInterfaces)
            {
                if (!first)
                {
                    this.headerWriter.Write(", ");
                }

                this.Save(
                    interfaceOfClass,
                    this.headerWriter,
                    SavingOptions.UseFullyQualifiedNames | SavingOptions.RemovePointer);

                first = false;
            }

            this.headerWriter.WriteLine();
            this.headerWriter.WriteLine('{');
            this.headerWriter.Indent++;

            // before save all static initializers
            this.SaveFieldInitializersIntoDefaultConstructor(false);

            this.@switch(@class.ChildElements);

            this.headerWriter.Indent--;

            this.headerWriter.WriteLine("};");

            this.ClassContext = null;
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="fieldDeclaration">
        /// The field declaration.
        /// </param>
        private void Save(Field fieldDeclaration)
        {
            var isStatic = fieldDeclaration.Declaration.ContainsModifier(CsTokenType.Static);

            this.Save(fieldDeclaration.AccessModifier);
            this.headerWriter.Write(": ");

            if (isStatic)
            {
                this.headerWriter.Write("static ");
            }

            this.SwitchStreams();

            this.saveVariablesMode = SaveVariablesMode.DoNotSaveInitializers;

            this.Save(fieldDeclaration.VariableDeclarationStatement);

            this.saveVariablesMode = SaveVariablesMode.Default;

            this.SwitchStreams();

            this.headerWriter.WriteLine(';');

            if (isStatic || fieldDeclaration.VariableDeclarationStatement.Constant)
            {
                this.saveVariablesMode = SaveVariablesMode.DefaultSourceInitializers;

                this.Save(fieldDeclaration.VariableDeclarationStatement);

                this.saveVariablesMode = SaveVariablesMode.Default;

                this.cppWriter.WriteLine(';');
                this.cppWriter.WriteLine();
            }
        }

        private void Save(Event @event)
        {
            // 1 to destHeader
            if (!(@event.Parent is Namespace))
            {
                this.SaveModifiersBefore(@event);
            }

            // 1 to destHeader
            this.headerWriter.Write("event ");

            this.Save(new TypeResolver(@event.EventHandlerType, this), this.headerWriter, SavingOptions.UseFullyQualifiedNames);
            this.headerWriter.Write(" ");
            this.headerWriter.Write(@event.Declaration.Name);

            this.headerWriter.WriteLine(';');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="delegate">
        /// The delegate.
        /// </param>
        private void Save(Delegate @delegate)
        {
            // 1 to destHeader
            if (!(@delegate.Parent is Namespace))
            {
                this.SaveModifiersBefore(@delegate);
            }

            // 1 to destHeader
            this.headerWriter.Write("delegate ");
            this.headerWriter.Write(@delegate.Declaration.ElementType);
            this.headerWriter.Write(" ");
            this.headerWriter.Write(@delegate.Declaration.Name);
            this.headerWriter.Write(@delegate.Parameters);

            this.headerWriter.WriteLine(';');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="methodDeclaration">
        /// The method declaration.
        /// </param>
        private void Save(Method methodDeclaration)
        {
            this.CurrentParameters = methodDeclaration.Parameters;

            var methodName = methodDeclaration.Declaration.Name;

            var lastDot = methodName.LastIndexOf('.');
            var nameWithoutNamespace = lastDot != -1
                                           ? methodName.Substring(lastDot + 1, methodName.Length - lastDot - 1)
                                           : methodName;
            var methodInterfaceNamespace = lastDot != -1 ? methodName.Substring(0, lastDot) : string.Empty;

            var isGenericMethod = nameWithoutNamespace.Contains('<') && nameWithoutNamespace.Contains('>');
            var isOperator = methodName.StartsWith("operator ");
            if (isOperator)
            {
                methodName = methodName.Substring("operator ".Length);

                switch (methodName)
                {
                    case "==":
                        methodName = RelationalExpression.Operator.EqualTo.ToString();
                        break;
                    case "!=":
                        methodName = RelationalExpression.Operator.NotEqualTo.ToString();
                        break;
                    case ">":
                        methodName = RelationalExpression.Operator.GreaterThan.ToString();
                        break;
                    case ">=":
                        methodName = RelationalExpression.Operator.GreaterThanOrEqualTo.ToString();
                        break;
                    case "<":
                        methodName = RelationalExpression.Operator.LessThan.ToString();
                        break;
                    case "<=":
                        methodName = RelationalExpression.Operator.LessThanOrEqualTo.ToString();
                        break;
                    case "+":
                        methodName = "Add";
                        break;
                    case "-":
                        methodName = "Remove";
                        break;
                    default:
                        break;
                }

                methodName = string.Concat("op_", methodName);
                nameWithoutNamespace = methodName;
            }

            // decide which mode 1/2 files to use;
            var restoreStateIsCppInHeader = this.IsCPPInHeader;
            if (!this.IsCPPInHeader)
            {
                this.IsCPPInHeader = isGenericMethod;
            }

            if (this.ClassContext.IsInterface)
            {
                this.IsCPPInHeader = true;
            }

            // 1 to destHeader
            this.Save(methodDeclaration.AccessModifier);
            this.headerWriter.Write(": ");

            var isImplicit = false;
            var isExplicit = false;

            // if generic, write template here
            if (isGenericMethod)
            {
                this.BuildDeclaretionTemplatePart(methodName);
                this.headerWriter.Write(this.BuildDeclaretionTemplatePart(methodName));
            }

            methodDeclaration.SaveDeclatationsAfterModifiers(this.headerWriter);

            // ASD: HACK
            if (methodDeclaration.ReturnType != null)
            {
                // 1 to destHeader
                this.Save(methodDeclaration.ReturnType);

                if (!this.IsCPPInHeader)
                {
                    // 2 to Source
                    this.Save(methodDeclaration.ReturnType, this.cppWriter, SavingOptions.None);
                }
            }
            else
            {
                // check if it is implicit or explicit operators
                isImplicit = methodDeclaration.Declaration.ContainsModifier(CsTokenType.Implicit);
                isExplicit = methodDeclaration.Declaration.ContainsModifier(CsTokenType.Explicit);
                if (isImplicit || isExplicit)
                {
                    // var returnType = new ResolvedTypeReference(typeConvertTypeName, this);
                    // this.Save(returnType, this.destHeader, false);
                    if (!this.IsCPPInHeader)
                    {
                        // 2 to Source
                        // this.Save(returnType, this.destCPP, false);
                    }
                }
            }

            var cppName = nameWithoutNamespace;
            cppName = isGenericMethod ? cppName.Split('<')[0] : cppName;

            // add namespace of interface if any
            if (!string.IsNullOrEmpty(methodInterfaceNamespace))
            {
                // var typeResolverReference = new ResolvedTypeReference(methodInterfaceNamespace, this);

                // cppName = String.Concat(typeResolverReference.CNameFullyQualifiedName, "::", cppName);
            }

            if (isImplicit)
            {
                cppName = "op_Implicit";
            }
            else if (isExplicit)
            {
                cppName = "op_Explicit";
            }

            // 1 to destHeader
            this.headerWriter.Write(' ');
            this.headerWriter.Write(cppName);

            // 2 to Source
            if (!this.IsCPPInHeader)
            {
                this.cppWriter.Write(' ');
                this.cppWriter.Write(this.currentClassNamespace);
                this.cppWriter.Write("::");
                this.cppWriter.Write(cppName);
            }

            this.SaveParametersAndBody(methodDeclaration);

            if (!this.IsCPPInHeader)
            {
                this.cppWriter.WriteLine();
            }

            this.CurrentParameters = null;

            this.IsCPPInHeader = restoreStateIsCppInHeader;
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="constructor">
        /// The constructor.
        /// </param>
        private void Save(Constructor @constructor)
        {
            if (this.HasAnyPointerType(@constructor))
            {
                return;
            }

            // 1 to destHeader
            this.SaveModifiersBefore(@constructor);

            // 1 to destHeader
            this.headerWriter.Write(this.GetNameBase(@constructor.Declaration.Name, this.ClassContext.IsTemplate));

            if (!this.IsCPPInHeader)
            {
                // 2 to Source
                this.cppWriter.Write(this.currentClassNamespace);
                this.cppWriter.Write("::");
                this.cppWriter.Write(@constructor.Declaration.Name);
            }

            var hasParameters = this.SaveParameters(@constructor);
            this.SaveModifiersAfter(@constructor);

            if (!this.IsCPPInHeader)
            {
                this.headerWriter.WriteLine(';');
            }

            this.SaveDefaultConstructorInitializer(constructor, this.SaveFieldInitializersIntoDefaultConstructor(false));

            this.Save((ICodeUnit)@constructor);

            this.cppWriter.WriteLine();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="constructorInitializerStatement">
        /// The constructor initializer statement.
        /// </param>
        /// <returns>
        /// The save.
        /// </returns>
        private bool Save(ConstructorInitializerStatement constructorInitializerStatement)
        {
            return false;
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="destructor">
        /// The destructor.
        /// </param>
        private void Save(Destructor destructor)
        {
            // 1 to destHeader
            this.SaveModifiersBefore(destructor);

            // 1 to destHeader
            this.headerWriter.Write(destructor.Declaration.Name);

            // 2 to Source
            this.cppWriter.Write(this.currentClassNamespace);
            this.cppWriter.Write("::");
            this.cppWriter.Write(destructor.Declaration.Name);

            this.SaveParametersAndBody(destructor);

            this.cppWriter.WriteLine();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="indexer">
        /// The indexer.
        /// </param>
        private void Save(Indexer @indexer)
        {
            var @propertyName = @indexer.Declaration.Name;
            this.headerWriter.Write(
                string.Format(
                    "__declspec(property({0}{2}{1})) ",
                    (@indexer.GetAccessor != null)
                        ? string.Concat("get = ", this.GetFunctionPrefix(@indexer.GetAccessor), "this")
                        : string.Empty,
                    (@indexer.SetAccessor != null)
                        ? string.Concat("put = ", this.GetFunctionPrefix(@indexer.SetAccessor), "this")
                        : string.Empty,
                    (@indexer.GetAccessor != null && @indexer.SetAccessor != null) ? ", " : string.Empty));

            if (@indexer.ReturnType != null)
            {
                // 1 to destHeader
                this.Save(@indexer.ReturnType);
            }

            // 1 to destHeader
            this.headerWriter.Write(" ");

            // this.destHeader.Write(@indexer.Declaration.Name);
            this.headerWriter.Write("Default");
            this.headerWriter.WriteLine("[];");

            if (@indexer.GetAccessor != null)
            {
                this.Save(@indexer.GetAccessor);
            }

            if (@indexer.SetAccessor != null)
            {
                this.Save(@indexer.SetAccessor);
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="property">
        /// The property.
        /// </param>
        private void Save(Property @property)
        {
            this.Save(@property.AccessModifier);
            this.headerWriter.Write(": ");

            if (@property.IsStatic())
            {
                this.headerWriter.Write("static ");
            }

            this.headerWriter.Write("property ");

            var @propertyName = @property.Declaration.Name.Replace(".", "::");

            if (@property.ReturnType != null)
            {
                // 1 to destHeader
                this.Save(@property.ReturnType);
            }

            // 1 to destHeader
            this.headerWriter.Write(" ");
            this.headerWriter.Write(@propertyName);

            this.headerWriter.WriteLine();
            this.headerWriter.WriteLine("{");
            this.headerWriter.Indent++;

            if (@property.GetAccessor != null)
            {
                this.Save(property.GetAccessor);
            }

            if (@property.SetAccessor != null)
            {
                this.Save(property.SetAccessor);
            }

            this.headerWriter.Indent--;
            this.headerWriter.WriteLine("};");
        }

        private void Save(Attribute attribute, IndentedTextWriter writer)
        {
            writer.Write("[");

            var first = true;
            foreach (var attributeExpression in attribute.AttributeExpressions)
            {
                if (!first)
                {
                    writer.Write(", ");
                }
                else
                {
                    first = false;
                }

                var methodInvocationExpression = attributeExpression.Initialization as MethodInvocationExpression;
                if (methodInvocationExpression != null)
                {
                    var resolvedType = new TypeResolver(methodInvocationExpression.Name.Text, this);
                    writer.Write(
                        resolvedType.IsResolved
                            ? resolvedType.GetCXFullyQualifiedName(SavingOptions.UseFullyQualifiedNames)
                            : methodInvocationExpression.Name.Text.Replace(".", "::"));

                    writer.Write('(');

                    this.SwitchStreams();
                    this.Save(methodInvocationExpression.Arguments);
                    this.SwitchStreams();

                    writer.Write(')');

                    continue;
                }

                var memberAccessExpression = attributeExpression.Initialization as MemberAccessExpression;
                if (memberAccessExpression != null)
                {
                    var resolvedType = new TypeResolver(memberAccessExpression.Text, this);
                    writer.Write(
                        resolvedType.IsResolved
                            ? resolvedType.GetCXFullyQualifiedName(SavingOptions.UseFullyQualifiedNames)
                            : memberAccessExpression.Text.Replace(".", "::"));
                }
            }

            writer.WriteLine("]");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="accessor">
        /// The accessor.
        /// </param>
        private void Save(Accessor accessor)
        {
            // 1 to destHeader
            var accessorName = accessor.Tokens.First.Value.Text;
            if (accessorName == "get" || accessorName == "set")
            {
                this.SaveModifiersBefore((CsElement)accessor.Parent);
            }
            else
            {
                this.SaveModifiersBefore(accessor);
            }

            TypeToken typeToken = null;

            typeToken = accessor.ReturnType;

            if (accessor.AccessorType == AccessorType.Get && typeToken != null)
            {
                // 1 to destHeader
                this.Save(typeToken);

                if (!this.IsCPPInHeader)
                {
                    // 2 to Source
                    this.Save(typeToken, this.cppWriter, SavingOptions.None);
                }
            }
            else
            {
                // 1 to destHeader
                this.headerWriter.Write("void");

                if (!this.IsCPPInHeader)
                {
                    // 2 to Source
                    this.cppWriter.Write("void");
                }
            }

            var functionName = accessor.Declaration.Name;
            this.headerWriter.Write(" ");
            this.headerWriter.Write(functionName);

            if (!this.IsCPPInHeader)
            {
                var propertyName = ((StyleCop.CSharp.CsElement)(accessor.Parent)).Declaration.Name;

                // 2 to Source
                this.cppWriter.Write(" ");
                this.cppWriter.Write(this.currentClassNamespace);
                this.cppWriter.Write("::");
                this.cppWriter.Write(propertyName);
                this.cppWriter.Write("::");
                this.cppWriter.Write(functionName);
            }

            this.SaveParameters(accessor);
            this.SaveModifiersAfter(accessor);

            if (!this.IsCPPInHeader)
            {
                this.headerWriter.WriteLine(';');
            }

            this.Save((ICodeUnit)accessor);

            this.cppWriter.WriteLine();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="childStatements">
        /// The child statements.
        /// </param>
        private void Save(ICollection<Statement> childStatements)
        {
            this.Save(childStatements, this.cppWriter);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="childStatements">
        /// The child statements.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private void Save(IEnumerable<Statement> childStatements, IndentedTextWriter writer)
        {
            foreach (var childStatement in childStatements)
            {
                this.Save(childStatement);
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="blockStatement">
        /// The block statement.
        /// </param>
        private void Save(BlockStatement blockStatement)
        {
            this.Save((ICodeUnit)blockStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="codeUnit">
        /// The code unit.
        /// </param>
        private void Save(ICodeUnit codeUnit)
        {
            this.Save(codeUnit, SaveICodeUnit.Statements);
        }

        private void Save(ICodeUnit codeUnit, SaveICodeUnit saveICodeUnit)
        {
            if (saveICodeUnit.HasFlag(SaveICodeUnit.IfNotEmpty))
            {
                if (!saveICodeUnit.HasFlag(SaveICodeUnit.Expressions) && codeUnit.ChildStatements.Count == 0)
                {
                    return;
                }
                else if (saveICodeUnit.HasFlag(SaveICodeUnit.Expressions) && codeUnit.ChildExpressions.Count == 0)
                {
                    return;
                }
            }

            this.SetMarkBeginOfBlock();

            this.cppWriter.WriteLine();

            if (!saveICodeUnit.HasFlag(SaveICodeUnit.NoBrackets))
            {
                this.cppWriter.WriteLine("{");
            }

            if (!saveICodeUnit.HasFlag(SaveICodeUnit.Expressions))
            {
                this.Save(codeUnit.ChildStatements);
            }
            else
            {
                this.Save(codeUnit.ChildExpressions);
            }

            if (!saveICodeUnit.HasFlag(SaveICodeUnit.NoBrackets))
            {
                if (!saveICodeUnit.HasFlag(SaveICodeUnit.NoNewLine))
                {
                    this.cppWriter.WriteLine("}");
                }
                else
                {
                    this.cppWriter.Write("}");
                }
            }

            this.SetMarkEndOfBlock();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="enum">
        /// The enum.
        /// </param>
        private void Save(Enum @enum)
        {
            this.headerWriter.WriteLine(@enum.Name);
            this.headerWriter.WriteLine("{");

            this.headerWriter.Indent++;

            var first = true;
            foreach (var enumItem in @enum.Items)
            {
                var csElement = enumItem as CsElement;

                if (!first)
                {
                    this.headerWriter.WriteLine(",");
                }

                var index = enumItem.Name.LastIndexOf(' ');
                if (index >= 0)
                {
                    this.headerWriter.Write(enumItem.Name.Substring(index + 1));

                    if (enumItem.Initialization != null)
                    {
                        this.headerWriter.Write(" = ");

                        this.SwitchStreams();

                        //this.SetLogicalExpressionPrefixForOthers(string.Concat(parts[parts.Length - 2], '_'));

                        this.@switch(enumItem.Initialization);

                        this.ClearLogicalExpressionPrefixForOthers();

                        this.SwitchStreams();
                    }
                }

                first = false;
            }

            this.headerWriter.Indent--;

            this.headerWriter.WriteLine();
            this.headerWriter.WriteLine("};");
            this.headerWriter.WriteLine();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parenthesizedExpression">
        /// The parenthesized expression.
        /// </param>
        private void Save(ParenthesizedExpression parenthesizedExpression)
        {
            this.cppWriter.Write('(');
            @switch(parenthesizedExpression.InnerExpression);
            this.cppWriter.Write(')');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void Save(IParameterContainer parameters)
        {
            Debug.Assert(parameters != null, "parameters is null");

            this.Save(parameters.Parameters, this.headerWriter, SavingOptions.UseFullyQualifiedNames);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="savingOptions">
        /// The saving Options.
        /// </param>
        private void Save(IParameterContainer parameters, IndentedTextWriter writer, SavingOptions savingOptions)
        {
            this.Save(parameters.Parameters, writer, savingOptions);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private void Save(IList<Argument> arguments)
        {
            var first = true;
            foreach (var argument in arguments)
            {
                if (!first)
                {
                    this.cppWriter.Write(", ");
                }

                this.Save(argument);

                first = false;
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="argument">
        /// The argument.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private void Save(Argument argument)
        {
            if (argument.Modifiers != ParameterModifiers.None)
            {
                this.Save(argument.Modifiers, this.cppWriter);
                this.SaveModifiersAfter(argument.Modifiers, this.cppWriter);

                this.cppWriter.Write(' ');
            }

            @switch(argument.Expression);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        private void Save(IList<Parameter> parameters)
        {
            this.Save(parameters, this.headerWriter, SavingOptions.UseFullyQualifiedNames);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="savingOptions">
        /// The saving Options.
        /// </param>
        private void Save(IList<Parameter> parameters, IndentedTextWriter writer, SavingOptions savingOptions)
        {
            writer.Write('(');

            var first = true;
            foreach (var parameter in parameters)
            {
                if (!first)
                {
                    writer.Write(", ");
                }

                this.Save(parameter, writer, savingOptions);

                first = false;
            }

            writer.Write(')');
        }

        private void Save(Preprocessor preprocessor)
        {
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void Save(Parameter parameter)
        {
            this.Save(parameter, this.headerWriter, SavingOptions.UseFullyQualifiedNames);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="savingOptions">
        /// The saving Options.
        /// </param>
        private void Save(Parameter parameter, IndentedTextWriter writer, SavingOptions savingOptions)
        {
            this.Save(parameter.Modifiers, writer);

            if (parameter.Type != null)
            {
                this.Save(parameter.Type, writer, savingOptions);
                this.SaveModifiersAfter(parameter.Modifiers, writer);

                writer.Write(' ');
            }

            writer.Write(parameter.Name);

            if (parameter.Type != null)
            {
                this.SaveSuffix(parameter.Type, null, writer, false);
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="parameterModifiers">
        /// The parameter modifiers.
        /// </param>
        /// <param name="indentedTextWriter">
        /// The indented text writer.
        /// </param>
        private void Save(ParameterModifiers parameterModifiers, IndentedTextWriter indentedTextWriter)
        {
            var parameterModifiersString = string.Empty;

            switch (parameterModifiers)
            {
                case ParameterModifiers.Out:

                    // parameterModifiersString = "out ";
                    break;
                case ParameterModifiers.Params:
                    parameterModifiersString = "/*params*/ ";
                    break;
                case ParameterModifiers.Ref:

                    // parameterModifiersString = "ref ";
                    break;
                case ParameterModifiers.This:
                    parameterModifiersString = "this ";
                    break;
                case ParameterModifiers.None:
                default:
                    return;
            }

            indentedTextWriter.Write(parameterModifiersString);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="localVariable">
        /// The local variable.
        /// </param>
        private void Save(VariableDeclarationStatement localVariable)
        {
            var storesaveVariablesMode = this.saveVariablesMode;

            if (localVariable.Constant)
            {
                // must be static for C++ as well
                if (storesaveVariablesMode == SaveVariablesMode.DefaultSourceInitializers)
                {
                    this.cppWriter.Write("const ");
                }
                else
                {
                    this.cppWriter.Write("const static ");
                }
            }

            this.Save(localVariable.InnerExpression);

            this.saveVariablesMode = storesaveVariablesMode;
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="variableDeclarationExpression">
        /// The variable declaration expression.
        /// </param>
        private void Save(VariableDeclarationExpression variableDeclarationExpression)
        {
            var savingOptions = this.saveVariablesMode != SaveVariablesMode.DoNotSaveInitializers
                                    ? SavingOptions.None
                                    : SavingOptions.UseFullyQualifiedNames;

            this.Save(variableDeclarationExpression.Type, this.cppWriter, savingOptions);

            if (this.saveVariablesMode == SaveVariablesMode.AppendRightReferene)
            {
                this.cppWriter.Write("&&");
            }

            this.cppWriter.Write(' ');

            this.Save(variableDeclarationExpression.Declarators);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="declarators">
        /// The declarators.
        /// </param>
        private void Save(ICollection<VariableDeclaratorExpression> declarators)
        {
            var first = true;
            foreach (var variableDeclaratorExpression in declarators)
            {
                if (!first)
                {
                    this.cppWriter.Write(", ");
                }

                this.Save(variableDeclaratorExpression);

                first = false;
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="variableDeclaratorExpression">
        /// The variable declarator expression.
        /// </param>
        private void Save(VariableDeclaratorExpression variableDeclaratorExpression)
        {
            if (this.saveVariablesMode == SaveVariablesMode.DefaultSourceInitializers)
            {
                var nakedClassName = this.GetNameBase(this.ClassContext.Class.Declaration.Name);
                this.cppWriter.Write(nakedClassName);
                this.cppWriter.Write("::");
            }

            if (this.saveVariablesMode == SaveVariablesMode.Default)
            {
                this.SavePrefix(
                    variableDeclaratorExpression.ParentVariable.Type,
                    variableDeclaratorExpression.Initializer,
                    this.cppWriter);
            }

            this.cppWriter.Write(variableDeclaratorExpression.Identifier);

            if (this.saveVariablesMode != SaveVariablesMode.Default)
            {
                this.SaveSuffix(
                    variableDeclaratorExpression.ParentVariable.Type,
                    variableDeclaratorExpression.Initializer,
                    this.cppWriter,
                    true);
            }

            if (variableDeclaratorExpression.Initializer != null
                && this.saveVariablesMode != SaveVariablesMode.DoNotSaveInitializers)
            {
                this.cppWriter.Write(" = ");
                @switch(variableDeclaratorExpression.Initializer);
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="uncheckedExpression">
        /// The unchecked expression.
        /// </param>
        private void Save(UncheckedExpression uncheckedExpression)
        {
            this.cppWriter.WriteLine("/* unchecked */");
            @switch(uncheckedExpression.InternalExpression);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="uncheckedStatement">
        /// The unchecked statement.
        /// </param>
        private void Save(UncheckedStatement uncheckedStatement)
        {
            this.cppWriter.WriteLine("/* unchecked */");
            this.Save(uncheckedStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="usingStatement">
        /// The using statement.
        /// </param>
        private void Save(UsingStatement usingStatement)
        {
            var isLiteral = usingStatement.Resource is LiteralExpression;

            // this.cppWriter.Write("/* using */");
            if (!isLiteral)
            {
                this.cppWriter.WriteLine();
                this.cppWriter.WriteLine("{");
                this.cppWriter.Indent++;

                @switch(usingStatement.Resource);
                this.cppWriter.Write(";");
            }

            this.Save(usingStatement.EmbeddedStatement);

            if (!isLiteral)
            {
                // get disposable interface
                var variableDeclaration = usingStatement.Resource as VariableDeclarationExpression;
                if (variableDeclaration != null)
                {
                    foreach (var varDecl in variableDeclaration.Declarators)
                    {
                        this.cppWriter.Write("dynamic_cast<");
                        this.Save(
                            new TypeResolver("IDisposable", this), this.cppWriter, SavingOptions.UseFullyQualifiedNames);
                        this.cppWriter.Write(">(");
                        this.cppWriter.Write(varDecl.Identifier);
                        this.cppWriter.WriteLine(")->Dispose();");
                    }
                }

                this.cppWriter.Indent--;
                this.cppWriter.WriteLine("}");
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="fixedStatement">
        /// The fixed statement.
        /// </param>
        private void Save(FixedStatement fixedStatement)
        {
            this.cppWriter.WriteLine("/* fixed */");
            this.cppWriter.WriteLine("{");
            this.cppWriter.Indent++;

            this.Save(fixedStatement.FixedVariable);
            this.cppWriter.Write(";");

            this.SetMarkEndOfBlock();

            this.Save(fixedStatement.EmbeddedStatement);

            this.cppWriter.Indent--;
            this.cppWriter.WriteLine("}");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="unsafeStatement">
        /// The unsafe statement.
        /// </param>
        private void Save(UnsafeStatement unsafeStatement)
        {
            this.cppWriter.Write("/* unsafe */");
            this.Save((Statement)unsafeStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="literalExpression">
        /// The literal expression.
        /// </param>
        private void Save(LiteralExpression literalExpression)
        {
            switch (literalExpression.Token.CsTokenType)
            {
                case CsTokenType.Abstract:
                    break;
                case CsTokenType.Add:
                    break;
                case CsTokenType.Alias:
                    break;
                case CsTokenType.As:
                    break;
                case CsTokenType.Ascending:
                    break;
                case CsTokenType.Attribute:
                    break;
                case CsTokenType.AttributeColon:
                    break;
                case CsTokenType.Base:

                    ////if (this.currentBaseClass != null)
                    ////{
                    ////    this.Save(this.currentBaseClass, this.destCPP, true);
                    ////    return;
                    ////}
                    this.cppWriter.Write("__super::");
                    return;
                case CsTokenType.BaseColon:
                    break;
                case CsTokenType.Break:
                    break;
                case CsTokenType.By:
                    break;
                case CsTokenType.Case:
                    break;
                case CsTokenType.Catch:
                    break;
                case CsTokenType.Checked:
                    break;
                case CsTokenType.Class:
                    break;
                case CsTokenType.CloseAttributeBracket:
                    break;
                case CsTokenType.CloseCurlyBracket:
                    break;
                case CsTokenType.CloseGenericBracket:
                    break;
                case CsTokenType.CloseParenthesis:
                    break;
                case CsTokenType.CloseSquareBracket:
                    break;
                case CsTokenType.Comma:
                    break;
                case CsTokenType.Const:
                    break;
                case CsTokenType.Continue:
                    break;
                case CsTokenType.Default:
                    break;
                case CsTokenType.DefaultValue:
                    break;
                case CsTokenType.Delegate:
                    break;
                case CsTokenType.Descending:
                    break;
                case CsTokenType.DestructorTilde:
                    break;
                case CsTokenType.Do:
                    break;
                case CsTokenType.Else:
                    break;
                case CsTokenType.EndOfLine:
                    break;
                case CsTokenType.Enum:
                    break;
                case CsTokenType.Equals:
                    break;
                case CsTokenType.Event:
                    break;
                case CsTokenType.Explicit:
                    break;
                case CsTokenType.Extern:
                    break;
                case CsTokenType.ExternDirective:
                    break;
                case CsTokenType.False:
                    break;
                case CsTokenType.Finally:
                    break;
                case CsTokenType.Fixed:
                    break;
                case CsTokenType.For:
                    break;
                case CsTokenType.Foreach:
                    break;
                case CsTokenType.From:
                    break;
                case CsTokenType.Get:
                    break;
                case CsTokenType.Goto:
                    break;
                case CsTokenType.Group:
                    break;
                case CsTokenType.If:
                    break;
                case CsTokenType.Implicit:
                    break;
                case CsTokenType.In:
                    break;
                case CsTokenType.Interface:
                    break;
                case CsTokenType.Internal:
                    break;
                case CsTokenType.Into:
                    break;
                case CsTokenType.Is:
                    break;
                case CsTokenType.Join:
                    break;
                case CsTokenType.LabelColon:
                    break;
                case CsTokenType.Let:
                    break;
                case CsTokenType.Lock:
                    break;
                case CsTokenType.MultiLineComment:
                    break;
                case CsTokenType.Namespace:
                    break;
                case CsTokenType.New:
                    break;
                case CsTokenType.Null:

                    this.cppWriter.Write("nullptr");
                    return;

                case CsTokenType.NullableTypeSymbol:
                    break;
                case CsTokenType.Number:
                    break;
                case CsTokenType.On:
                    break;
                case CsTokenType.OpenAttributeBracket:
                    break;
                case CsTokenType.OpenCurlyBracket:
                    break;
                case CsTokenType.OpenGenericBracket:
                    break;
                case CsTokenType.OpenParenthesis:
                    break;
                case CsTokenType.OpenSquareBracket:
                    break;
                case CsTokenType.Operator:
                    break;
                case CsTokenType.OperatorSymbol:
                    break;
                case CsTokenType.OrderBy:
                    break;
                case CsTokenType.Other:
                    this.cppWriter.Write(this.letteralOthersPrefix);
                    break;
                case CsTokenType.Out:
                    break;
                case CsTokenType.Override:
                    break;
                case CsTokenType.Params:
                    break;
                case CsTokenType.Partial:
                    break;
                case CsTokenType.PreprocessorDirective:
                    break;
                case CsTokenType.Private:
                    break;
                case CsTokenType.Protected:
                    break;
                case CsTokenType.Public:
                    break;
                case CsTokenType.Readonly:
                    break;
                case CsTokenType.Ref:
                    break;
                case CsTokenType.Remove:
                    break;
                case CsTokenType.Return:
                    break;
                case CsTokenType.Sealed:
                    break;
                case CsTokenType.Select:
                    break;
                case CsTokenType.Semicolon:
                    break;
                case CsTokenType.Set:
                    break;
                case CsTokenType.SingleLineComment:
                    break;
                case CsTokenType.Sizeof:
                    break;
                case CsTokenType.Stackalloc:
                    break;
                case CsTokenType.Static:
                    break;
                case CsTokenType.String:
                    this.cppWriter.Write(literalExpression.ToString());

                    // var typeResolverReference = new ResolvedTypeReference("System.String", this);
                    // this.AddToRequiredTypeRefsForBody(
                    // typeResolverReference.CSType, typeResolverReference.FullyQualifiedName);
                    return;
                case CsTokenType.Struct:
                    break;
                case CsTokenType.Switch:
                    break;
                case CsTokenType.This:
                    break;
                case CsTokenType.Throw:
                    break;
                case CsTokenType.True:
                    break;
                case CsTokenType.Try:
                    break;
                case CsTokenType.Typeof:
                    break;
                case CsTokenType.Unchecked:
                    break;
                case CsTokenType.Unsafe:
                    break;
                case CsTokenType.Using:
                    break;
                case CsTokenType.UsingDirective:
                    break;
                case CsTokenType.Virtual:
                    break;
                case CsTokenType.Volatile:
                    break;
                case CsTokenType.Where:
                    break;
                case CsTokenType.WhereColon:
                    break;
                case CsTokenType.While:
                    break;
                case CsTokenType.WhileDo:
                    break;
                case CsTokenType.WhiteSpace:
                    break;
                case CsTokenType.XmlHeader:
                    break;
                case CsTokenType.XmlHeaderLine:
                    break;
                case CsTokenType.Yield:
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(literalExpression.ToString());
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="arithmeticExpression">
        /// The arithmetic expression.
        /// </param>
        private void Save(ArithmeticExpression arithmeticExpression)
        {
            @switch(arithmeticExpression.LeftHandSide);
            this.Save(arithmeticExpression.OperatorType);
            @switch(arithmeticExpression.RightHandSide);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(ArithmeticExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case ArithmeticExpression.Operator.Addition:
                    operatorString = "+";
                    break;
                case ArithmeticExpression.Operator.Division:
                    operatorString = "/";
                    break;
                case ArithmeticExpression.Operator.LeftShift:
                    operatorString = "<<";
                    break;
                case ArithmeticExpression.Operator.Mod:
                    operatorString = "%";
                    break;
                case ArithmeticExpression.Operator.Multiplication:
                    operatorString = "*";
                    break;
                case ArithmeticExpression.Operator.RightShift:
                    operatorString = ">>";
                    break;
                case ArithmeticExpression.Operator.Subtraction:
                    operatorString = "-";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(' ');
            this.cppWriter.Write(operatorString);
            this.cppWriter.Write(' ');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="conditionalLogicalExpression">
        /// The conditional logical expression.
        /// </param>
        private void Save(ConditionalLogicalExpression conditionalLogicalExpression)
        {
            @switch(conditionalLogicalExpression.LeftHandSide);
            this.Save(conditionalLogicalExpression.OperatorType);
            @switch(conditionalLogicalExpression.RightHandSide);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(ConditionalLogicalExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case ConditionalLogicalExpression.Operator.And:
                    operatorString = "&&";
                    break;
                case ConditionalLogicalExpression.Operator.Or:
                    operatorString = "||";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(' ');
            this.cppWriter.Write(operatorString);
            this.cppWriter.Write(' ');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="conditionalExpression">
        /// The conditional expression.
        /// </param>
        private void Save(ConditionalExpression conditionalExpression)
        {
            this.cppWriter.Write('(');
            @switch(conditionalExpression.Condition);
            this.cppWriter.Write(") ? ");
            @switch(conditionalExpression.TrueExpression);
            this.cppWriter.Write(" : ");
            @switch(conditionalExpression.FalseExpression);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="anonymousMethodExpression">
        /// The anonymous method expression.
        /// </param>
        private void Save(AnonymousMethodExpression anonymousMethodExpression)
        {
            this.cppWriter.Write("[=]");
            this.Save(anonymousMethodExpression.Parameters, this.cppWriter, SavingOptions.None /* SavingOptions.ApplyReference */);
            this.Save(anonymousMethodExpression, SaveICodeUnit.NoNewLine);
            this.SetMarkBeginOfBlock();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="logicalExpression">
        /// The logical expression.
        /// </param>
        private void Save(LogicalExpression logicalExpression)
        {
            @switch(logicalExpression.LeftHandSide);
            this.Save(logicalExpression.OperatorType);
            @switch(logicalExpression.RightHandSide);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(LogicalExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case LogicalExpression.Operator.And:
                    operatorString = "&";
                    break;
                case LogicalExpression.Operator.Or:
                    operatorString = "|";
                    break;
                case LogicalExpression.Operator.Xor:
                    operatorString = "^";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(' ');
            this.cppWriter.Write(operatorString);
            this.cppWriter.Write(' ');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="memberAccessExpression">
        /// The member access expression.
        /// </param>
        private void Save(MemberAccessExpression memberAccessExpression)
        {
            // todo: investigate why it returns multiple result with the same methods
            var expressionReturnTypeResolver = new ExpressionReturnTypeResolver(this);
            var resolvedCodeElements = expressionReturnTypeResolver.Resolve(memberAccessExpression.LeftHandSide);
            var members = resolvedCodeElements.ToList();

            var resolvedTypeReference = new TypeResolver(memberAccessExpression.LeftHandSide.Text, this);

            if (resolvedTypeReference.IsClassName || resolvedTypeReference.IsEnum)
            {
                bool isFullyQualified = memberAccessExpression.LeftHandSide.Text.StartsWith(resolvedTypeReference.Namespace);
                this.cppWriter.Write(
                    isFullyQualified
                        ? resolvedTypeReference.GetCXFullyQualifiedName(SavingOptions.None)
                        : resolvedTypeReference.GetCxName(SavingOptions.None));
            }
            else
            {
                @switch(memberAccessExpression.LeftHandSide);
            }

            this.Save(this.GetActualMemberAccess(memberAccessExpression, resolvedTypeReference, members, expressionReturnTypeResolver.IsMemberFound));

            @switch(memberAccessExpression.RightHandSide);
        }

        // todo: reduce it
        private MemberAccessExpression.Operator GetActualMemberAccess(
            MemberAccessExpression memberAccessExpression, TypeResolver typeResolverReference, IEnumerable<ExpressionReturnTypeResolver.ResolvedContextItem> resolvedCodeElements, bool memberWasFound)
        {
            if (memberAccessExpression.OperatorType == MemberAccessExpression.Operator.Dot)
            {
                if (typeResolverReference.IsResolved && (typeResolverReference.IsClassName || typeResolverReference.IsEnum))
                {
                    return MemberAccessExpression.Operator.QualifiedAlias;
                }

                if (resolvedCodeElements.Any())
                {
                    var first = resolvedCodeElements.First();
                    var typeDefinitionMetadataICodeElementAdapterBase = first.Type as TypeDefinitionMetadataICodeElementAdapterBase;
                    if (typeDefinitionMetadataICodeElementAdapterBase != null && typeDefinitionMetadataICodeElementAdapterBase.IsValueType)
                    {
                        return MemberAccessExpression.Operator.Dot;
                    }

                    return MemberAccessExpression.Operator.Pointer;
                }

                if (memberAccessExpression.LeftHandSide is LiteralExpression 
                    && typeResolverReference.IsResolved
                    && !typeResolverReference.IsFieldOrVariable)
                {
                    return MemberAccessExpression.Operator.QualifiedAlias;
                }


                var current = memberAccessExpression;
                while (current != null)
                {
                    if (current.LeftHandSide is MethodInvocationExpression)
                    {
                        return MemberAccessExpression.Operator.Dot;
                    }

                    current = current.LeftHandSide as MemberAccessExpression;
                }

                return memberWasFound ? MemberAccessExpression.Operator.Dot : MemberAccessExpression.Operator.QualifiedAlias;
            }

            return memberAccessExpression.OperatorType;
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(MemberAccessExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case MemberAccessExpression.Operator.Dot:
                    operatorString = ".";
                    break;
                case MemberAccessExpression.Operator.Pointer:
                    operatorString = "->";
                    break;
                case MemberAccessExpression.Operator.QualifiedAlias:
                    operatorString = "::";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(operatorString);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="relationalExpression">
        /// The relational expression.
        /// </param>
        private void Save(RelationalExpression relationalExpression)
        {
            @switch(relationalExpression.LeftHandSide);
            this.Save(relationalExpression.OperatorType);
            @switch(relationalExpression.RightHandSide);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(RelationalExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case RelationalExpression.Operator.EqualTo:
                    operatorString = "==";
                    break;
                case RelationalExpression.Operator.NotEqualTo:
                    operatorString = "!=";
                    break;
                case RelationalExpression.Operator.GreaterThan:
                    operatorString = ">";
                    break;
                case RelationalExpression.Operator.GreaterThanOrEqualTo:
                    operatorString = ">=";
                    break;
                case RelationalExpression.Operator.LessThan:
                    operatorString = "<";
                    break;
                case RelationalExpression.Operator.LessThanOrEqualTo:
                    operatorString = "<=";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(' ');
            this.cppWriter.Write(operatorString);
            this.cppWriter.Write(' ');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="unaryExpression">
        /// The unary expression.
        /// </param>
        private void Save(UnaryExpression unaryExpression)
        {
            this.Save(unaryExpression.OperatorType);
            @switch(unaryExpression.Value);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(UnaryExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case UnaryExpression.Operator.BitwiseCompliment:
                    operatorString = "~";
                    break;
                case UnaryExpression.Operator.Negative:
                    operatorString = "-";
                    break;
                case UnaryExpression.Operator.Not:
                    operatorString = "!";
                    break;
                case UnaryExpression.Operator.Positive:
                    operatorString = "+";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(operatorString);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="newExpression">
        /// The new expression.
        /// </param>
        private void Save(NewExpression newExpression)
        {
            var methodInvocationExpression = newExpression.TypeCreationExpression as MethodInvocationExpression;
            if (methodInvocationExpression != null)
            {
                this.cppWriter.Write("ref new ");

                this.Save(methodInvocationExpression.Name.Text, this.cppWriter, SavingOptions.RemovePointer);
                this.cppWriter.Write("(");
                this.Save(methodInvocationExpression.Arguments);
                this.cppWriter.Write(")");

                @switch(newExpression.InitializerExpression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="arrayInitializerExpression">
        /// The array initializer expression.
        /// </param>
        private void Save(ArrayInitializerExpression arrayInitializerExpression)
        {
            this.cppWriter.WriteLine();
            this.cppWriter.WriteLine("{");

            this.cppWriter.Indent++;

            this.Save(arrayInitializerExpression.Initializers, this.cppWriter);

            this.cppWriter.Indent--;

            this.cppWriter.WriteLine();
            this.cppWriter.Write("}");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="newArrayExpression">
        /// The new array expression.
        /// </param>
        private void Save(NewArrayExpression newArrayExpression)
        {
            this.cppWriter.Write("new ");
            @switch(newArrayExpression.Type);

            if (newArrayExpression.Initializer != null)
            {
                @switch(newArrayExpression.Initializer);
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="arrayAccessExpression">
        /// The array access expression.
        /// </param>
        private void Save(ArrayAccessExpression arrayAccessExpression)
        {
            @switch(arrayAccessExpression.Array);
            this.cppWriter.Write("[");
            this.Save(arrayAccessExpression.Arguments);
            this.cppWriter.Write("]");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="asExpression">
        /// The as expression.
        /// </param>
        private void Save(AsExpression asExpression)
        {
            this.cppWriter.Write("dynamic_cast<");
            this.Save(asExpression.Type, this.cppWriter, SavingOptions.None);
            this.cppWriter.Write(">(");
            @switch(asExpression.Value);
            this.cppWriter.Write(")");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="typeofExpression">
        /// The typeof expression.
        /// </param>
        private void Save(TypeofExpression typeofExpression)
        {
            this.cppWriter.Write("TypeName(");
            this.Save(new TypeResolver(typeofExpression.Type, this), this.cppWriter, SavingOptions.RemovePointer);
            this.cppWriter.Write("::typeid)");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="isExpression">
        /// The is expression.
        /// </param>
        private void Save(IsExpression isExpression)
        {
            this.cppWriter.Write("dynamic_cast<");
            this.Save(isExpression.Type, this.cppWriter, SavingOptions.None);
            this.cppWriter.Write(">(");
            @switch(isExpression.Value);
            this.cppWriter.Write(") != nullptr");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="checkedExpression">
        /// The checked expression.
        /// </param>
        private void Save(CheckedExpression checkedExpression)
        {
            this.cppWriter.Write("(");
            @switch(checkedExpression.InternalExpression);
            this.cppWriter.Write(")");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="lockStatement">
        /// The lock statement.
        /// </param>
        private void Save(LockStatement lockStatement)
        {
            this.cppWriter.Write("Monitor::Enter(");
            @switch(lockStatement.LockedExpression);
            this.cppWriter.WriteLine(");");

            @switch(lockStatement.EmbeddedStatement);

            this.cppWriter.Write("Monitor::Exit(");
            @switch(lockStatement.LockedExpression);
            this.cppWriter.WriteLine(");");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="incrementExpression">
        /// The increment expression.
        /// </param>
        private void Save(IncrementExpression incrementExpression)
        {
            if (incrementExpression.Type == IncrementExpression.IncrementType.Prefix)
            {
                this.cppWriter.Write("++");
                @switch(incrementExpression.Value);
            }
            else
            {
                @switch(incrementExpression.Value);
                this.cppWriter.Write("++");
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="decrementExpression">
        /// The decrement expression.
        /// </param>
        private void Save(DecrementExpression decrementExpression)
        {
            if (decrementExpression.Type == DecrementExpression.DecrementType.Prefix)
            {
                this.cppWriter.Write("--");
                @switch(decrementExpression.Value);
            }
            else
            {
                @switch(decrementExpression.Value);
                this.cppWriter.Write("--");
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="castExpression">
        /// The cast expression.
        /// </param>
        private void Save(CastExpression castExpression)
        {
            this.cppWriter.Write("safe_cast<");
            this.Save(castExpression.Type, this.cppWriter, SavingOptions.None);
            this.cppWriter.Write(">(");
            @switch(castExpression.CastedExpression);
            this.cppWriter.Write(")");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="unsafeAccessExpression">
        /// The unsafe access expression.
        /// </param>
        private void Save(UnsafeAccessExpression unsafeAccessExpression)
        {
            this.Save(unsafeAccessExpression.OperatorType);
            @switch(unsafeAccessExpression.Value);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(UnsafeAccessExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case UnsafeAccessExpression.Operator.AddressOf:
                    operatorString = "&";
                    break;
                case UnsafeAccessExpression.Operator.Dereference:
                    operatorString = "*";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(operatorString);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="expressionStatement">
        /// The expression statement.
        /// </param>
        private void Save(ExpressionStatement expressionStatement)
        {
            @switch(expressionStatement.Expression);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="lambdaExpression">
        /// The lambda expression.
        /// </param>
        private void Save(LambdaExpression lambdaExpression)
        {
            this.cppWriter.Write("[=]");
            this.Save(lambdaExpression.Parameters, this.cppWriter, SavingOptions.None /*SavingOptions.ApplyReference*/);

            var methodInvocationExpression = lambdaExpression.AnonymousFunctionBody as MethodInvocationExpression;
            if (methodInvocationExpression != null)
            {
                this.SetMarkBeginOfBlock();
                this.cppWriter.WriteLine();
                this.cppWriter.WriteLine("{");
                this.cppWriter.Indent++;
                this.Save(methodInvocationExpression);
                this.cppWriter.WriteLine(";");
                this.cppWriter.Indent--;
                this.cppWriter.Write("}");

                return;
            }

            this.Save(lambdaExpression.AnonymousFunctionBody, SaveICodeUnit.NoNewLine);
            this.SetMarkBeginOfBlock();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="assignmentExpression">
        /// The assignment expression.
        /// </param>
        private void Save(AssignmentExpression assignmentExpression)
        {
            @switch(assignmentExpression.LeftHandSide);
            this.Save(assignmentExpression.OperatorType);
            @switch(assignmentExpression.RightHandSide);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="operator">
        /// The operator.
        /// </param>
        private void Save(AssignmentExpression.Operator @operator)
        {
            var operatorString = string.Empty;

            switch (@operator)
            {
                case AssignmentExpression.Operator.AndEquals:
                    operatorString = "&=";
                    break;
                case AssignmentExpression.Operator.DivisionEquals:
                    operatorString = "/=";
                    break;
                case AssignmentExpression.Operator.Equals:
                    operatorString = "=";
                    break;
                case AssignmentExpression.Operator.LeftShiftEquals:
                    operatorString = "<<=";
                    break;
                case AssignmentExpression.Operator.MinusEquals:
                    operatorString = "-=";
                    break;
                case AssignmentExpression.Operator.ModEquals:
                    operatorString = "%=";
                    break;
                case AssignmentExpression.Operator.MultiplicationEquals:
                    operatorString = "*=";
                    break;
                case AssignmentExpression.Operator.OrEquals:
                    operatorString = "|=";
                    break;
                case AssignmentExpression.Operator.PlusEquals:
                    operatorString = "+=";
                    break;
                case AssignmentExpression.Operator.RightShiftEquals:
                    operatorString = ">>=";
                    break;
                case AssignmentExpression.Operator.XorEquals:
                    operatorString = "^=";
                    break;
                default:
                    break;
            }

            this.cppWriter.Write(' ');
            this.cppWriter.Write(operatorString);
            this.cppWriter.Write(' ');
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="methodInvocationExpression">
        /// The method invocation expression.
        /// </param>
        private void Save(MethodInvocationExpression methodInvocationExpression)
        {
            if (methodInvocationExpression.Name.Text == "base")
            {
                this.Save(this.currentBaseClass, this.cppWriter, SavingOptions.RemovePointer);
            }
            else
            {
                if (methodInvocationExpression.Name is LiteralExpression
                    && methodInvocationExpression.Name.Text.Contains('<'))
                {
                    // you need to process generic types
                    this.Save(methodInvocationExpression.Name.Text, this.cppWriter, SavingOptions.RemovePointer);
                }
                else
                {
                    @switch(methodInvocationExpression.Name);
                }
            }

            this.cppWriter.Write("(");
            @switch(methodInvocationExpression.Arguments);
            this.cppWriter.Write(")");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="forStatement">
        /// The for statement.
        /// </param>
        private void Save(ForStatement forStatement)
        {
            this.cppWriter.Write("for (");
            @switch(forStatement.Initializers);
            this.cppWriter.Write("; ");
            @switch(forStatement.Condition);
            this.cppWriter.Write("; ");
            @switch(forStatement.Iterators);
            this.cppWriter.Write(")");

            this.Save(forStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="switchStatement">
        /// The switch statement.
        /// </param>
        private void Save(SwitchStatement switchStatement)
        {
            this.cppWriter.Write("switch (");
            @switch(switchStatement.SwitchItem);
            this.cppWriter.WriteLine(")");

            this.SetMarkBeginOfBlock();

            this.cppWriter.WriteLine("{");

            @switch(switchStatement.CaseStatements);

            this.cppWriter.Indent++;
            this.cppWriter.WriteLine();
            @switch(switchStatement.DefaultStatement);
            this.cppWriter.Indent--;

            this.cppWriter.WriteLine("}");

            this.SetMarkEndOfBlock();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="switchCaseStatement">
        /// The switch case statement.
        /// </param>
        private void Save(SwitchCaseStatement switchCaseStatement)
        {
            this.cppWriter.Write("case ");
            @switch(switchCaseStatement.Identifier);
            this.cppWriter.WriteLine(":");

            this.Save(switchCaseStatement, SaveICodeUnit.IfNotEmpty | SaveICodeUnit.NoBrackets);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="switchDefaultStatement">
        /// The switch default statement.
        /// </param>
        private void Save(SwitchDefaultStatement switchDefaultStatement)
        {
            this.cppWriter.WriteLine("default: ");

            this.Save(switchDefaultStatement, SaveICodeUnit.IfNotEmpty | SaveICodeUnit.NoBrackets);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="foreachStatement">
        /// The foreach statement.
        /// </param>
        private void Save(ForeachStatement foreachStatement)
        {
            this.cppWriter.Write("for (");

            this.saveVariablesMode = SaveVariablesMode.AppendRightReferene;
            this.Save(foreachStatement.Variable);
            this.saveVariablesMode = SaveVariablesMode.Default;

            this.cppWriter.Write(" : ");
            @switch(foreachStatement.Item);
            this.cppWriter.Write(")");

            this.Save(foreachStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="whileStatement">
        /// The while statement.
        /// </param>
        private void Save(WhileStatement whileStatement)
        {
            this.cppWriter.Write("while (");
            @switch(whileStatement.ConditionExpression);
            this.cppWriter.Write(")");

            this.Save(whileStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="doWhileStatement">
        /// The do while statement.
        /// </param>
        private void Save(DoWhileStatement doWhileStatement)
        {
            this.cppWriter.Write("do");

            this.Save(doWhileStatement.EmbeddedStatement);

            this.cppWriter.Write("while (");
            @switch(doWhileStatement.ConditionalExpression);
            this.cppWriter.WriteLine(")");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="tryStatement">
        /// The try statement.
        /// </param>
        private void Save(TryStatement tryStatement)
        {
            this.cppWriter.Write("try");

            this.Save((Statement)tryStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="catchStatement">
        /// The catch statement.
        /// </param>
        private void Save(CatchStatement catchStatement)
        {
            this.cppWriter.Write("catch (");
            @switch(catchStatement.CatchExpression);

            if (catchStatement.CatchExpression is LiteralExpression)
            {
                this.cppWriter.Write("^");
            }

            this.cppWriter.Write(")");

            this.Save((Statement)catchStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="finallyStatement">
        /// The finally statement.
        /// </param>
        private void Save(FinallyStatement finallyStatement)
        {
            this.cppWriter.Write("finally");

            this.Save((Statement)finallyStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="breakStatement">
        /// The break statement.
        /// </param>
        private void Save(BreakStatement breakStatement)
        {
            this.cppWriter.Write("break");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="continueStatement">
        /// The continue statement.
        /// </param>
        private void Save(ContinueStatement continueStatement)
        {
            this.cppWriter.Write("continue");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="throwStatement">
        /// The throw statement.
        /// </param>
        private void Save(ThrowStatement throwStatement)
        {
            this.cppWriter.Write("throw ");
            @switch(throwStatement.ThrownExpression);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="awaitStatement">
        /// The await statement.
        /// </param>
        private void Save(AwaitStatement awaitStatement)
        {
            @switch(awaitStatement.AwaitValue);
            this.cppWriter.Write(".get()");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="awaitExpression">
        /// The await expression.
        /// </param>
        private void Save(AwaitExpression awaitExpression)
        {
            @switch(awaitExpression.InternalExpression);
            this.cppWriter.Write("->GetResults()");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="expressions">
        /// The expressions.
        /// </param>
        private void Save(IEnumerable<Expression> expressions)
        {
            this.Save(expressions, this.cppWriter);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="expressions">
        /// The expressions.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private void Save(IEnumerable<Expression> expressions, IndentedTextWriter writer)
        {
            var first = true;
            foreach (var expression in expressions)
            {
                if (!first)
                {
                    writer.Write(", ");
                }

                this.@switch(expression);

                first = false;
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="ifStatement">
        /// The if statement.
        /// </param>
        private void Save(IfStatement ifStatement)
        {
            this.cppWriter.Write("if (");
            @switch(ifStatement.ConditionExpression);
            this.cppWriter.Write(")");

            this.Save(ifStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="elseStatement">
        /// The else statement.
        /// </param>
        private void Save(ElseStatement elseStatement)
        {
            this.cppWriter.Write("else");

            this.Save(elseStatement.EmbeddedStatement);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="labelStatement">
        /// The label statement.
        /// </param>
        private void Save(LabelStatement labelStatement)
        {
            @switch(labelStatement.Identifier);
            this.cppWriter.WriteLine(":");
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="gotoStatement">
        /// The goto statement.
        /// </param>
        private void Save(GotoStatement gotoStatement)
        {
            this.cppWriter.Write("goto ");
            @switch(gotoStatement.Identifier);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="emptyElement">
        /// The empty element.
        /// </param>
        private void Save(EmptyElement emptyElement)
        {
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="emptyStatement">
        /// The empty statement.
        /// </param>
        private void Save(EmptyStatement emptyStatement)
        {
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="typeToken">
        /// The type token.
        /// </param>
        private void Save(TypeToken typeToken)
        {
            this.Save(typeToken, this.headerWriter, SavingOptions.UseFullyQualifiedNames);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="typeReference">
        /// The type reference.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="savingOptions">
        /// The saving Options.
        /// </param>
        private void Save(TypeToken typeReference, IndentedTextWriter writer, SavingOptions savingOptions)
        {
            this.Save(new TypeResolver(typeReference, this), writer, savingOptions);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="typeReferenceText">
        /// The type reference text.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="savingOptions">
        /// The saving Options.
        /// </param>
        private void Save(string typeReferenceText, IndentedTextWriter writer, SavingOptions savingOptions)
        {
            this.Save(new TypeResolver(typeReferenceText, this), writer, savingOptions);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="typeResolver">
        /// The resolved type reference.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="savingOptions">
        /// The saving Options.
        /// </param>
        private void Save(TypeResolver typeResolver, IndentedTextWriter writer, SavingOptions savingOptions)
        {
            writer.Write(
                savingOptions.HasFlag(SavingOptions.UseFullyQualifiedNames) || !typeResolver.IsNamespaceInUsingDerictives
                    ? typeResolver.GetCxFullyQualifiedType(savingOptions)
                    : typeResolver.GetCxType(savingOptions));
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="returnStatement">
        /// The return statement.
        /// </param>
        private void Save(ReturnStatement returnStatement)
        {
            this.cppWriter.Write("return ");
            @switch(returnStatement.ReturnValue);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="accessModifierType">
        /// The access modifier type.
        /// </param>
        private void Save(AccessModifierType accessModifierType)
        {
            if (this.ClassContext.IsInterface)
            {
                return;
            }

            switch (accessModifierType)
            {
                case AccessModifierType.Internal:
                    this.headerWriter.Write("internal");
                    break;
                case AccessModifierType.Private:
                    this.headerWriter.Write("private");
                    break;
                case AccessModifierType.Protected:
                    this.headerWriter.Write("protected");
                    break;
                case AccessModifierType.ProtectedAndInternal:
                    this.headerWriter.Write("protected");
                    break;
                case AccessModifierType.ProtectedInternal:
                    this.headerWriter.Write("protected");
                    break;
                case AccessModifierType.Public:
                    this.headerWriter.Write("public");
                    break;
                default:
                    this.headerWriter.Write("private");
                    break;
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="statement">
        /// The statement.
        /// </param>
        private void Save(Statement statement)
        {
            var blockStatement = statement as BlockStatement;
            if (blockStatement != null)
            {
                // this.SetMarkBeginOfBlock();
                this.Save(blockStatement);

                // this.SetMarkEndOfBlock();
            }
            else
            {
                this.cppWriter.Indent++;

                this.SetMarkBeginOfNonBlockStatement(statement);

                if (!@switch(statement))
                {
                    this.ClearMarkBeginOfNonBlockStatement();
                }
                else
                {
                    this.SetMarkEndOfStatement();
                }

                this.cppWriter.Indent--;
            }
        }

        /// <summary>
        /// The save default constructor initializer.
        /// </summary>
        /// <param name="constructor">
        /// The constructor.
        /// </param>
        /// <param name="startedInitilizer">
        /// The started initilizer.
        /// </param>
        private void SaveDefaultConstructorInitializer(Constructor constructor, bool startedInitilizer)
        {
            if (@constructor.Initializer != null)
            {
                this.cppWriter.Indent++;

                if (!startedInitilizer)
                {
                    this.cppWriter.WriteLine(" : ");
                }
                else
                {
                    this.cppWriter.WriteLine(",");
                }

                this.Save(@constructor.Initializer);

                this.cppWriter.Indent--;
            }
        }

        /// <summary>
        /// The save field declarator expression into constructor.
        /// </summary>
        /// <param name="variableDeclaratorExpression">
        /// The variable declarator expression.
        /// </param>
        /// <param name="first">
        /// The first.
        /// </param>
        /// <returns>
        /// The save field declarator expression into constructor.
        /// </returns>
        private bool SaveFieldDeclaratorExpressionIntoConstructor(
            VariableDeclaratorExpression variableDeclaratorExpression, bool first)
        {
            if (variableDeclaratorExpression.Initializer == null)
            {
                return false;
            }

            if (!first)
            {
                this.cppWriter.WriteLine(',');
            }

            this.cppWriter.Write(variableDeclaratorExpression.Identifier);

            if (variableDeclaratorExpression.Initializer != null)
            {
                this.cppWriter.Write("(");
                @switch(variableDeclaratorExpression.Initializer);
                this.cppWriter.Write(")");
            }

            return true;
        }

        /// <summary>
        /// The save field declarators into constructor.
        /// </summary>
        /// <param name="declarators">
        /// The declarators.
        /// </param>
        /// <param name="saveStatic">
        /// The save static.
        /// </param>
        /// <param name="firstInit">
        /// The first init.
        /// </param>
        /// <param name="startedInitilizer">
        /// The started initilizer.
        /// </param>
        /// <returns>
        /// The save field declarators into constructor.
        /// </returns>
        private bool SaveFieldDeclaratorsIntoConstructor(
            ICollection<VariableDeclaratorExpression> declarators,
            bool saveStatic,
            bool firstInit,
            ref bool startedInitilizer)
        {
            var res = true;
            var first = true;
            foreach (var variableDeclaratorExpression in declarators)
            {
                if (!saveStatic)
                {
                    if (first && !startedInitilizer && variableDeclaratorExpression.Initializer != null)
                    {
                        this.cppWriter.Indent++;

                        this.cppWriter.WriteLine(" : ");

                        startedInitilizer = true;
                    }
                    else if (!first)
                    {
                        this.cppWriter.Write(", ");
                    }
                }

                if (saveStatic)
                {
                    this.SaveStaticFieldDeclaratorExpressionIntoConstructor(variableDeclaratorExpression);

                    first = false;
                }
                else
                {
                    res &= this.SaveFieldDeclaratorExpressionIntoConstructor(variableDeclaratorExpression, firstInit);

                    if (res)
                    {
                        first = false;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// The save field initializers into default constructor.
        /// </summary>
        /// <param name="saveStaticAndConstAsStatis">
        /// The save static and const as statis.
        /// </param>
        /// <returns>
        /// The save field initializers into default constructor.
        /// </returns>
        private bool SaveFieldInitializersIntoDefaultConstructor(bool saveStaticAndConstAsStatis)
        {
            // interate all fields with initializers;
            var first = true;
            var startedInitilizer = false;
            foreach (var codeElement in this.ClassContext.Class.ChildElements)
            {
                if (codeElement is Field)
                {
                    var field = (Field)codeElement;

                    if (
                        !(saveStaticAndConstAsStatis
                          &&
                          (field.Declaration.ContainsModifier(CsTokenType.Static)
                           || field.Declaration.ContainsModifier(CsTokenType.Const))))
                    {
                        continue;
                    }

                    if (this.SaveFieldDeclaratorsIntoConstructor(
                        field.VariableDeclarationStatement.Declarators,
                        saveStaticAndConstAsStatis,
                        first,
                        ref startedInitilizer))
                    {
                        first = false;

                        if (saveStaticAndConstAsStatis)
                        {
                            this.cppWriter.WriteLine(";");
                            this.cppWriter.WriteLine();
                        }
                    }
                }
            }

            if (!first && !saveStaticAndConstAsStatis)
            {
                this.cppWriter.Indent--;
            }

            return startedInitilizer;
        }

        /// <summary>
        /// The save modifiers after.
        /// </summary>
        /// <param name="csharpElement">
        /// The csharp element.
        /// </param>
        private void SaveModifiersAfter(CsElement csharpElement)
        {
            if (csharpElement.Declaration.ContainsModifier(CsTokenType.Override))
            {
                this.headerWriter.Write(" override");
            }
        }

        /// <summary>
        /// The save modifiers after.
        /// </summary>
        /// <param name="parameterModifiers">
        /// The parameter modifiers.
        /// </param>
        /// <param name="indentedTextWriter">
        /// The indented text writer.
        /// </param>
        private void SaveModifiersAfter(ParameterModifiers parameterModifiers, IndentedTextWriter indentedTextWriter)
        {
            var parameterModifiersString = string.Empty;

            switch (parameterModifiers)
            {
                case ParameterModifiers.Out:
                    parameterModifiersString = "&";
                    break;
                case ParameterModifiers.Params:
                    break;
                case ParameterModifiers.Ref:
                    parameterModifiersString = "&";
                    break;
                case ParameterModifiers.This:
                    break;
                case ParameterModifiers.None:
                default:
                    return;
            }

            indentedTextWriter.Write(parameterModifiersString);
        }

        /// <summary>
        /// The save modifiers before.
        /// </summary>
        /// <param name="csharpElement">
        /// The csharp element.
        /// </param>
        private void SaveModifiersBefore(CsElement csharpElement)
        {
            this.Save(csharpElement.AccessModifier);
            this.headerWriter.Write(": ");

            csharpElement.SaveDeclatationsAfterModifiers(this.headerWriter);
        }

        /// <summary>
        /// The save parameters.
        /// </summary>
        /// <param name="csharpElement">
        /// The csharp element.
        /// </param>
        /// <returns>
        /// The save parameters.
        /// </returns>
        private bool SaveParameters(CsElement csharpElement)
        {
            return this.SaveParameters(csharpElement, this.IsCPPInHeader);
        }

        /// <summary>
        /// The save parameters.
        /// </summary>
        /// <param name="csharpElement">
        /// The csharp element.
        /// </param>
        /// <param name="headerOnly">
        /// The header only.
        /// </param>
        /// <returns>
        /// The save parameters.
        /// </returns>
        private bool SaveParameters(CsElement csharpElement, bool headerOnly)
        {
            var parameterContainer = csharpElement as IParameterContainer;

            if (parameterContainer != null)
            {
                // 1 to destHeader
                this.Save(parameterContainer);

                if (!headerOnly)
                {
                    // 2 to Source
                    this.Save(parameterContainer, this.cppWriter, SavingOptions.None);
                }

                return true;
            }
            else
            {
                this.headerWriter.Write("()");
                this.cppWriter.Write("()");
            }

            return false;
        }

        /// <summary>
        /// The save parameters and body.
        /// </summary>
        /// <param name="csharpElement">
        /// The csharp element.
        /// </param>
        private void SaveParametersAndBody(CsElement csharpElement)
        {
            this.SaveParameters(csharpElement);

            this.SaveModifiersAfter(csharpElement);

            if (!this.IsCPPInHeader || this.ClassContext.IsInterface)
            {
                this.headerWriter.WriteLine(';');

                if (this.ClassContext.IsInterface)
                {
                    return;
                }
            }

            this.Save(csharpElement);

            if (this.IsCPPInHeader)
            {
                this.headerWriter.WriteLine();
            }
        }

        /// <summary>
        /// The save prefix.
        /// </summary>
        /// <param name="typeReference">
        /// The type reference.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        private void SavePrefix(TypeToken typeReference, Expression expression, IndentedTextWriter writer)
        {
            var typeReferenceString = typeReference.ToString();

            var pos = typeReferenceString.IndexOf("[");
            if (pos != -1)
            {
                // CXXConverterLogic.WriteArrayPointerIndexes(expression, writer, typeReferenceString, pos);
            }
        }

        /// <summary>
        /// The save static field declarator expression into constructor.
        /// </summary>
        /// <param name="variableDeclaratorExpression">
        /// The variable declarator expression.
        /// </param>
        private void SaveStaticFieldDeclaratorExpressionIntoConstructor(
            VariableDeclaratorExpression variableDeclaratorExpression)
        {
            this.Save(variableDeclaratorExpression.ParentVariable.Type, this.cppWriter, SavingOptions.None);
            this.cppWriter.Write(" ");
            this.cppWriter.Write(this.currentClassNamespace);
            this.cppWriter.Write("::");
            this.cppWriter.Write(variableDeclaratorExpression.Identifier);
            this.SaveSuffix(
                variableDeclaratorExpression.ParentVariable.Type,
                variableDeclaratorExpression.Initializer,
                this.cppWriter);

            if (variableDeclaratorExpression.Initializer != null)
            {
                this.cppWriter.Write(" = ");
                @switch(variableDeclaratorExpression.Initializer);

                // this.destCPP.WriteLine(";");
            }
        }

        /// <summary>
        /// The save suffix.
        /// </summary>
        /// <param name="typeReference">
        /// The type reference.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="fieldDeclaration">
        /// The field declaration.
        /// </param>
        private void SaveSuffix(
            TypeToken typeReference, Expression expression, IndentedTextWriter writer, bool fieldDeclaration = true)
        {
            var typeReferenceString = typeReference.ToString();

            var pos = typeReferenceString.IndexOf("[");
            if (pos != -1)
            {
                // var typeResolverReference = new ResolvedTypeReference(typeReference, this);

                // CXXConverterLogic.WriteArrayIndexes(
                // expression, writer, typeReferenceString, pos, typeResolverReference, fieldDeclaration);
            }
        }

        /// <summary>
        /// The set logical expression prefix for others.
        /// </summary>
        /// <param name="prefix">
        /// The prefix.
        /// </param>
        private void SetLogicalExpressionPrefixForOthers(string prefix)
        {
            this.letteralOthersPrefix = prefix;
        }

        /// <summary>
        /// The set mark begin of block.
        /// </summary>
        private void SetMarkBeginOfBlock()
        {
            this.lastBlockStatement = false;
            this.lastColumn = false;
            this.startOfStatement = false;
        }

        /// <summary>
        /// The set mark begin of non block statement.
        /// </summary>
        /// <param name="statement">
        /// The statement.
        /// </param>
        private void SetMarkBeginOfNonBlockStatement(Statement statement)
        {
            if (this.startOfStatement || this.lastBlockStatement)
            {
                if (!this.IsJointStatement(statement))
                {
                    this.cppWriter.WriteLine();
                }
            }

            if (this.lastColumn && this.IsSeparatedStatement(statement))
            {
                this.cppWriter.WriteLine();
            }

            this.ClearMarkBeginOfNonBlockStatement();

            this.startOfStatement = true;
        }

        /// <summary>
        /// The set mark end of block.
        /// </summary>
        private void SetMarkEndOfBlock()
        {
            // show that last is {}
            this.lastBlockStatement = true;
            this.lastColumn = false;
            this.startOfStatement = false;
        }

        /// <summary>
        /// The set mark end of statement.
        /// </summary>
        private void SetMarkEndOfStatement()
        {
            if (!this.lastBlockStatement)
            {
                if (!this.lastColumn)
                {
                    this.cppWriter.WriteLine(";");
                }

                this.lastColumn = true;
            }

            this.startOfStatement = false;
        }

        /// <summary>
        /// The switch streams.
        /// </summary>
        private void SwitchStreams()
        {
            var tempDestHeader = this.headerWriter;
            this.headerWriter = this.cppWriter;
            this.cppWriter = tempDestHeader;
        }

        /// <summary>
        /// The switch.
        /// </summary>
        /// <param name="childElements">
        /// The child elements.
        /// </param>
        private void @switch(ICollection<CsElement> childElements)
        {
            foreach (var codeElement in childElements)
            {
                this.@switch(codeElement);
            }
        }

        /// <summary>
        /// The switch.
        /// </summary>
        /// <param name="childElements">
        /// The child elements.
        /// </param>
        /// <param name="newLine">
        /// The new line.
        /// </param>
        private void @switch(ICollection<CsElement> childElements, bool newLine)
        {
            var first = true;

            foreach (var codeElement in childElements)
            {
                if (!first && newLine)
                {
                    this.cppWriter.WriteLine();
                }

                this.@switch(codeElement);

                first = false;
            }
        }

        /// <summary>
        /// The switch.
        /// </summary>
        /// <param name="languageElement">
        /// The language element.
        /// </param>
        /// <returns>
        /// The switch.
        /// </returns>
        private bool @switch(object languageElement)
        {
            if (languageElement == null)
            {
                return false;
            }

            var methodInfo = GetType().GetMethod(
                "Save",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.ExactBinding,
                null,
                new[] { languageElement.GetType() },
                null);

            if (methodInfo == null)
            {
#if DEBUG
                var callStack = new StackTrace();
                var frame = callStack.GetFrame(2);
                var method = frame.GetMethod();
#endif
                try
                {
                    methodInfo = GetType().GetMethod(
                        "Save",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                        null,
                        new[] { languageElement.GetType() },
                        null);
                }
                catch (AmbiguousMatchException)
                {
                }
            }

            if (methodInfo == null)
            {
                return false;
            }

            Debug.Assert(methodInfo != null, "Can't find method");

            var ret = methodInfo.Invoke(this, new[] { languageElement });
            if (ret != null)
            {
                return (bool)ret;
            }

            return true;
        }

        #endregion
    }
}
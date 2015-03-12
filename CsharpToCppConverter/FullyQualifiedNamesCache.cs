// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FullyQualifiedNamesCache.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the FullyQualifiedNamesCache type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;

    using Converters.Adapters;
    using Converters.ComInterfaces.MetadataEnums;
    using Converters.Metadata;
    using StyleCop;
    using StyleCop.CSharp;

    using Delegate = StyleCop.CSharp.Delegate;
    using Enum = StyleCop.CSharp.Enum;

    public class FullyQualifiedNamesCache
    {
        #region Fields

        private readonly NamespaceNode root;

        #endregion

        #region Constructors and Destructors

        public FullyQualifiedNamesCache()
        {
            this.root = new NamespaceNode("Root");
        }

        #endregion

        #region Public Indexers

        public NamespaceNode this[string fullyQualifiedName]
        {
            get
            {
                var found =
                    this.root.FindSubNamespaceNode(
                        fullyQualifiedName.StartsWith("Root.") ? fullyQualifiedName.Substring("Root.".Length) : fullyQualifiedName, false);
                if (found == null)
                {
                    return null;
                }

                return found;
            }
        }

        #endregion

        #region Public Methods and Operators

        public void Add(ICodeElement codeElement)
        {
            this.root.Add(codeElement);
        }

        public NamespaceNode FindNamespaceNode(string namespaceName)
        {
            return this.root.FindSubNamespaceNode(namespaceName, false);
        }

        public NamespaceNode FindNamespaceNodeFromRoot(string namespaceName)
        {
            return this.root.Find(namespaceName, false);
        }

        public ICodeElement FindTypeInNamespace(string namespaceName, string typeName, bool deepSearch)
        {
            return this[string.Concat(namespaceName, '.', typeName)].CodeElement;
        }

        public bool IsNamespace(string namespaceName)
        {
            var namespaceNode = this[namespaceName];
            return namespaceNode != null && namespaceNode.CodeElement is Namespace;
        }

        public void LoadNamesFrom(CsDocument file)
        {
            this.Process(file.DocumentContents);
        }

        public void LoadNamesFrom(string winmdFilePath)
        {
            var metadataReader = new MetadataReader(winmdFilePath);
            foreach (var typeDefinition in metadataReader.EnumerateTypeDefinitions())
            {
                this.Add(new TypeDefinitionMetadataICodeElementAdapter(typeDefinition, null, metadataReader));
            }
        }

        #endregion

        #region Methods

        private void Process(ICodeElement codeElement)
        {
            this.Add(codeElement);

            this.Process(codeElement.ChildCodeElements);
        }

        private void Process(IEnumerable<ICodeElement> childCodeElements)
        {
            if (childCodeElements == null)
            {
                return;
            }

            foreach (var codeElement in childCodeElements)
            {
                this.Process(codeElement);
            }
        }

        #endregion

        [DebuggerDisplay("{name,nq}, {namespaceNodes.Count == 0 ? \"\" : \"Count = \" + namespaceNodes.Count.ToString() + \", \",nq}{FullName,nq}")]
        public class NamespaceNode
        {
            #region Fields

            private readonly string name;

            private readonly IDictionary<string, NamespaceNode> namespaceNodes;

            private readonly NamespaceNode parent;

            private ICodeElement codeElement;

            private IList<ICodeElement> codeElements;

            #endregion

            #region Constructors and Destructors

            public NamespaceNode(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("name");
                }

                Contract.EndContractBlock();

                this.name = name;
                this.namespaceNodes = new SortedList<string, NamespaceNode>();
            }

            public NamespaceNode(string name, NamespaceNode parent)
                : this(name)
            {
                if (parent == null)
                {
                    throw new ArgumentException("parent");
                }

                Contract.EndContractBlock();

                parent.namespaceNodes.Add(this.GetCanonicalName(), this);

                this.parent = parent;
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

            public IEnumerable<ICodeElement> CodeElements
            {
                get
                {
                    return this.codeElements;
                }
            }

            public string FullName
            {
                get
                {
                    if (this.parent == null)
                    {
                        return string.Empty;
                    }

                    var parentFullName = this.parent.FullName;

                    return !string.IsNullOrEmpty(parentFullName)
                               ? string.Concat(parentFullName, '.', this.name)
                               : this.name;
                }
            }

            public string FullNamespace
            {
                get
                {
                    return this.parent.FullName;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public IDictionary<string, NamespaceNode> NamespaceNodes
            {
                get
                {
                    return this.namespaceNodes;
                }
            }

            #endregion

            #region Public Indexers

            public NamespaceNode this[string nestedFullyQualifiedName]
            {
                get
                {
                    return this.FindSubNamespaceNode(nestedFullyQualifiedName, false);
                }
            }

            public IEnumerable<ICodeElement> IterateCodeElements()
            {
                if (this.codeElement != null)
                {
                    yield return this.codeElement;
                }

                if (this.codeElements != null)
                {
                    foreach (var codeElement in this.codeElements)
                    {
                        yield return codeElement;
                    }
                }
            }

            #endregion

            #region Methods

            internal void Add(ICodeElement codeElement)
            {
                if (codeElement == null)
                {
                    throw new ArgumentException("codeElement");
                }

                Contract.EndContractBlock();

                this.Add(codeElement.FullyQualifiedName, codeElement);
            }

            internal void Add(string path, ICodeElement codeElement)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("path");
                }

                if (codeElement == null)
                {
                    throw new ArgumentException("codeElement");
                }

                Contract.EndContractBlock();

                var declarations = codeElement is Namespace || codeElement is Class || codeElement is Struct
                                   || codeElement is Interface || codeElement is Enum || codeElement is Delegate
                                   || codeElement is Event || codeElement is MetadataICodeElementAdapter;

                // you should not add properties etc because you need types not parts of types like method. #
                // because Version type can be found as method Version
                // || codeElement is Field
                // || codeElement is Property
                // || codeElement is Method;
                if (declarations)
                {
                    this.Find(path, true).Assign(codeElement);
                }
            }

            // todo: allow only multiple partial classes
            internal void Assign(ICodeElement codeElement)
            {
                if (codeElement == null)
                {
                    throw new ArgumentException("codeElement");
                }

                Contract.EndContractBlock();

                if (this.codeElement == null)
                {
                    this.codeElement = codeElement;
                }
                else
                {                    
                    if (this.codeElements == null)
                    {
                        this.codeElements = new List<ICodeElement>();
                    }

                    var typeDefinitionMetadataICodeElementAdapter = codeElement as TypeDefinitionMetadataICodeElementAdapter;
                    if (typeDefinitionMetadataICodeElementAdapter != null
                        && !typeDefinitionMetadataICodeElementAdapter.TypeDefinition.Type.HasFlag(CorTypeAttr.Import))
                    {
                        this.codeElements.Add(this.codeElement);
                        this.codeElement = codeElement;
                    }
                    else
                    {
                        this.codeElements.Add(codeElement);
                    }
                }
            }

            internal NamespaceNode Find(string path, bool createIfEmpty)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("path");
                }

                Contract.EndContractBlock();

                string tailNames;
                var firstName = NamespaceFuncs.GetFirstAndTail(path, out tailNames);
                if (firstName.Equals(this.Name))
                {
                    if (string.IsNullOrEmpty(tailNames))
                    {
                        return this;
                    }

                    return this.FindSubNamespaceNode(tailNames, createIfEmpty);
                }

                throw new ArgumentOutOfRangeException("path");
            }

            internal NamespaceNode FindSubNamespaceNode(string path, bool createIfEmpty)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("path");
                }

                Contract.EndContractBlock();

                string tailNames;
                var firstNameOriginal = NamespaceFuncs.GetFirstAndTail(path, out tailNames);
                var firstName = GetCanonicalName(firstNameOriginal);

                NamespaceNode foundNamespaceNode;
                if (!this.namespaceNodes.TryGetValue(firstName, out foundNamespaceNode))
                {
                    if (createIfEmpty)
                    {
                        foundNamespaceNode = new NamespaceNode(firstName, this);
                    }
                    else
                    {
                        return null;

                        ////if (firstName == "Deployment")
                        ////{
                        ////    return null;
                        ////}

                        ////throw new ArgumentOutOfRangeException("path");
                    }
                }

                if (string.IsNullOrEmpty(tailNames))
                {
                    return foundNamespaceNode;
                }

                return foundNamespaceNode.FindSubNamespaceNode(tailNames, createIfEmpty);
            }

            private static string GetCanonicalName(string name)
            {
                var genericLevel = CXXConverterLogic.GetGenericsTypeNumber(name);

                if (genericLevel == 0)
                {
                    return name;
                }

                return string.Concat(name.Substring(0, name.IndexOf('<')), '`', genericLevel);
            }

            private string GetCanonicalName()
            {
                return GetCanonicalName(this.Name);
            }

            #endregion
        }

        internal class NamespaceFuncs
        {
            #region Public Methods and Operators

            public static string GetFirst(string path)
            {
                var firstName = path;

                var pos = path.IndexOf('.');
                if (pos != -1)
                {
                    firstName = path.Substring(0, pos);
                }

                return firstName;
            }

            public static string GetFirstAndTail(string path, out string tailNames)
            {
                var firstName = path;
                tailNames = string.Empty;

                var pos = path.IndexOf('.');
                if (pos != -1)
                {
                    firstName = path.Substring(0, pos);
                    tailNames = path.Substring(pos + 1);
                }
                else
                {
                    // check if it is name of mathod with encoded params
                    var methodParamsPos = path.IndexOf('%');
                    if (methodParamsPos != -1)
                    {
                        return path.Substring(0, methodParamsPos);
                    }

                    return path;
                }

                return firstName;
            }

            public static string GetTail(string path)
            {
                var tailNames = string.Empty;

                var pos = path.IndexOf('.');
                if (pos != -1)
                {
                    tailNames = path.Substring(pos + 1);
                }

                return tailNames;
            }

            #endregion
        }
    }
}
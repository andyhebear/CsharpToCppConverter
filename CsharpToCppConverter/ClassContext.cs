// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassContext.cs" company="">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   The class context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Converters
{
    using System.Diagnostics;

    using StyleCop.CSharp;

    /// <summary>
    /// The current class.
    /// </summary>
    public class ClassContext
    {
        #region Fields

        /// <summary>
        ///   The class.
        /// </summary>
        private readonly ClassBase @class;

        /// <summary>
        ///   The namespace node.
        /// </summary>
        private readonly FullyQualifiedNamesCache.NamespaceNode namespaceNode;

        /// <summary>
        ///   The is interface.
        /// </summary>
        private bool? isInterface;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassContext"/> class.
        /// </summary>
        /// <param name="class">
        /// The class.
        /// </param>
        /// <param name="fullyQualifiedNameCache">
        /// The fully qualified name cache.
        /// </param>
        public ClassContext(ClassBase @class, FullyQualifiedNamesCache fullyQualifiedNameCache)
        {
            this.@class = @class;
            this.namespaceNode = fullyQualifiedNameCache.FindNamespaceNodeFromRoot(this.Class.FullyQualifiedName);
            //Debug.Assert(this.namespaceNode != null);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets Class.
        /// </summary>
        public ClassBase Class
        {
            get
            {
                return this.@class;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether IsInterface.
        /// </summary>
        public bool IsInterface
        {
            get
            {
                if (this.isInterface.HasValue)
                {
                    return this.isInterface.Value;
                }

                this.isInterface = this.@class is Interface;

                return this.isInterface.Value;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether IsTemplate.
        /// </summary>
        public bool IsTemplate
        {
            get
            {
                return this.@class != null && this.@class.FullyQualifiedName.Contains("<");
            }
        }

        #endregion

        #region Public Indexers

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="nestedFullyQualifiedName">
        /// The nested fully qualified name.
        /// </param>
        /// <returns>
        /// The <see cref="NamespaceNode"/>.
        /// </returns>
        public FullyQualifiedNamesCache.NamespaceNode this[string nestedFullyQualifiedName]
        {
            get
            {
                if (this.namespaceNode == null)
                {
                    return null;
                }

                return this.namespaceNode[nestedFullyQualifiedName];
            }
        }

        #endregion
    }


}

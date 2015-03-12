using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StyleCop;
using StyleCop.CSharp;

namespace Converters
{
    /// <summary>
    /// The i names info holder.
    /// </summary>
    public interface INamesResolver
    {
        #region Public Properties

        /// <summary>
        ///   Gets CurrentParameters.
        /// </summary>
        IList<Parameter> CurrentParameters { get; }

        /// <summary>
        ///   Gets FullyQualifiedNames.
        /// </summary>
        FullyQualifiedNamesCache FullyQualifiedNames { get; }

        /// <summary>
        ///   Gets UsingDirectives.
        /// </summary>
        IList<string> UsingDirectives { get; }

        bool IsPlatformUsedInUsingDirectives { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The find code element.
        /// </summary>
        /// <param name="elementName">
        /// The element name.
        /// </param>
        /// <returns>
        /// </returns>
        FullyQualifiedNamesCache.NamespaceNode FindNamespaceNode(string elementName);

        /// <summary>
        /// The get current class namespace name.
        /// </summary>
        /// <returns>
        /// The get current class namespace name.
        /// </returns>
        string GetCurrentClassNamespaceName();

        /// <summary>
        /// The get current namespace name.
        /// </summary>
        /// <returns>
        /// The get current namespace name.
        /// </returns>
        string GetCurrentNamespaceName();

        #endregion
    }

}

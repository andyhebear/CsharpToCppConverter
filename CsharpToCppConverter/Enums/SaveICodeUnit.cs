// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveICodeUnit.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the SaveICodeUnit type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Converters.Enums
{
    using System;

    [Flags]
    internal enum SaveICodeUnit
    {
        Statements = 0,

        Expressions = 1,
        
        IfNotEmpty = 2,

        NoBrackets = 4, 

        NoNewLine = 8,        
    }
}
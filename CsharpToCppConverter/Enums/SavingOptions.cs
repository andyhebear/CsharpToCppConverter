// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavingOptions.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the SavingOptions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Converters.Enums
{
    using System;

    [Flags]
    public enum SavingOptions
    {
        None = 0, 

        UseFullyQualifiedNames = 1, 

        RemovePointer = 2, 

        ApplyReference = 4, 

        ApplyRvalueReference = 8
    }
}
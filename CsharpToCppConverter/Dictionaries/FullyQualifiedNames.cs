namespace Converters.Dictionaries
{
    using System.Collections.Generic;

    public class FullyQualifiedNames
    {
        public static readonly IDictionary<string, string> Map = new SortedDictionary<string, string>();

        static FullyQualifiedNames()
        {
            // fully Qualified Names
            Map.Add("void", "Platform.Void");
            Map.Add("bool", "Platform.Boolean");
            Map.Add("double", "Platform.Double");
            Map.Add("float", "Platform.Single");
            Map.Add("int", "Platform.Int32");
            Map.Add("long", "Platform.Int64");
            Map.Add("short", "Platform.Int16");
            Map.Add("char", "Platform.Char");
            Map.Add("sbyte", "Platform.SByte");
            Map.Add("byte", "Platform.Byte");
            Map.Add("uint", "Platform.UInt32");
            Map.Add("ulong", "Platform.UInt64");
            Map.Add("ushort", "Platform.UInt16");
            Map.Add("decimal", "Platform.Decimal");
            Map.Add("string", "Platform.String");
            Map.Add("object", "Platform.Object");
        }
    }
}
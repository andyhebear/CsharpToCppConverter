namespace Converters.Dictionaries
{
    using System.Collections.Generic;

    public class CsTypesToCppTypes
    {
        public static readonly IDictionary<string, string> Map = new SortedDictionary<string, string>();

        static CsTypesToCppTypes()
        {
            Map.Add("System.AttributeUsageAttribute", "Windows.Foundation.Metadata.AttributeUsageAttribute");
            Map.Add("System.AttributeTargets", "Windows.Foundation.Metadata.AttributeTargets");
            Map.Add("System.DateTimeOffset", "Windows.Foundation.DateTime");
            Map.Add("System.EventHandler`1", "Windows.Foundation.EventHandler`1");
            Map.Add("System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken", "Windows.Foundation.EventRegistrationToken");
            Map.Add("System.Exception", "Windows.Foundation.HResult");
            Map.Add("System.Nullable`1", "Windows.Foundation.IReference`1");
            Map.Add("System.TimeSpan", "Windows.Foundation.TimeSpan");
            Map.Add("System.Uri", "Windows.Foundation.Uri");
            Map.Add("System.IDisposable", "Windows.Foundation.IClosable");
            Map.Add("System.Collections.Generic.IEnumerable`1", "Windows.Foundation.Collections.IIterable`1");
            Map.Add("System.Collections.Generic.IList`1", "Windows.Foundation.Collections.IVector`1");
            Map.Add("System.Collections.Generic.IReadOnlyList`1", "Windows.Foundation.Collections.IVectorView`1");
            Map.Add("System.Collections.Generic.IDictionary`2", "Windows.Foundation.Collections.IMap`2");
            Map.Add("System.Collections.Generic.IReadOnlyDictionary`2", "Windows.Foundation.Collections.IMapView`2");
            Map.Add("System.Collections.Generic.KeyValuePair`2", "Windows.Foundation.Collections.IKeyValuePair`2");

            // additional map
            Map.Add("Box`1", "IBox`1");
            Map.Add("Vector`1", "IVector`1");
            Map.Add("Map`2", "IMap`2");
        }
    }
}

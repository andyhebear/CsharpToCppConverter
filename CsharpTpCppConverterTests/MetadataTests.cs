namespace SharpTpCppConverterTests
{
    using System.Linq;
    using Converters.Metadata;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MetadataTests
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void Load_Platform_Winmd_Test()
        {
            var metadataReader = new MetadataReader(@"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.VCLibs\11.0\References\CommonConfiguration\neutral\Platform.winmd");
            foreach (var typeDef in metadataReader.EnumerateTypeDefinitions())
            {
                Assert.IsNotNull(typeDef);
            }

            foreach (var typeRef in metadataReader.EnumerateTypeReferences())
            {
                Assert.IsNotNull(typeRef);
            }

            foreach (var moduleRef in metadataReader.EnumerateModuleReferences())
            {
                Assert.IsNotNull(moduleRef);
            }
        }

        [TestMethod]
        public void Load_Windows_Winmd_Test()
        {
            var metadataReader = new MetadataReader(@"C:\Program Files (x86)\Windows Kits\8.0\References\CommonConfiguration\Neutral\Windows.winmd");
            foreach (var typeDef in metadataReader.EnumerateTypeDefinitions())
            {
                Assert.IsNotNull(typeDef);
            }

            foreach (var typeRef in metadataReader.EnumerateTypeReferences())
            {
                Assert.IsNotNull(typeRef);
            }

            foreach (var moduleRef in metadataReader.EnumerateModuleReferences())
            {
                Assert.IsNotNull(moduleRef);
            }

            Assert.IsTrue(
                metadataReader.EnumerateTypeDefinitions().Any(x => x.FullName.Equals("Windows.Foundation.Collections.IMap`2")));
            Assert.IsTrue(
                metadataReader.EnumerateTypeDefinitions().Any(x => x.FullName.Equals("Windows.Foundation.Collections.IObservableMap`2")));
        }

        [TestMethod]
        public void ReadBlobUnsinged()
        {
            var bytes = new byte[] { 0x03, 0x7F, 0x80, 0x80, 0xAE, 0x57, 0xBF, 0xFF, 0xC0, 0x00, 0x40, 0x00, 0xDF, 0xFF, 0xFF, 0xFF };

            var reader = bytes.DecodeBlobAsUnsigned();

            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x03, reader.Current);
            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x7F, reader.Current);
            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x80, reader.Current);
            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x2E57, reader.Current);
            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x3FFF, reader.Current);
            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x4000, reader.Current);
            Assert.IsTrue(reader.MoveNext());
            Assert.AreEqual((ulong)0x1FFFFFFF, reader.Current);
            Assert.IsFalse(reader.MoveNext());
        }

        [TestMethod]
        public void DecodeTypeDefOrRefOrSpecEncoded()
        {
            int val = 0x49;

            var decodedToken = val.DecodeTypeDefOrRefOrSpecEncoded();

            Assert.AreEqual((int)0x01000012, decodedToken);
        }
    }
}

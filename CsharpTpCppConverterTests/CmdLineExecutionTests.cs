namespace SharpTpCppConverterTests
{
    using System.Collections.Generic;
    using System.Linq;

    using Converters;
    using Converters.Metadata;

    using CsharpToCppConverter;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CmdLineExecutionTests
    {
        private readonly string[] baseParams = new[]
        {
            @"/nologo", 
            @"/srcdir:C:\Users\Alexander\Documents\Visual Studio 2012\Projects\App1\App1",
            @"/libwinrtdirs:C:\Program Files (x86)\Windows Kits\8.0\References\CommonConfiguration\Neutral;C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcpackages", 
            @"/complist:App.xaml.cs",
            @"/complist:Common\BindableBase_.cs",
        };

        [TestMethod]
        public void ConvertBindableBaseTest()
        {
            Program.Main(baseParams);
        }

        [TestMethod]
        public void ConvertParams()
        {
            var dir = ParamsProcessor.ProcessArguments(baseParams);

            Assert.IsTrue(dir.ContainsKey("nologo"));
            Assert.IsTrue(dir.ContainsKey("srcdir"));
            Assert.IsTrue(dir.ContainsKey("libwinrtdirs"));
            Assert.IsTrue(dir.ContainsKey("complist"));

            Assert.IsTrue(dir["complist"] is IList<string>);
        }

        [TestMethod]
        public void CreatingInterpreters()
        {
            var arguments = ParamsProcessor.ProcessArguments(baseParams);
            var srcdir = arguments["srcdir"] as string;
            var libwinrtdirs = arguments["libwinrtdirs"] as string;
            var filePaths = arguments["complist"] as IList<string>;

            var interpreters = SharpToCppConverter.GetCsharpInterpretersFromFiles(filePaths, srcdir, srcdir, libwinrtdirs);

            Assert.IsTrue(interpreters.Count() == 2);
        }
    }
}

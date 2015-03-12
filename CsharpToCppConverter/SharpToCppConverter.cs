// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharpToCppConverter.cs" company="Mr O. Duzhar">
//   Mr O. Duzhar, Copyright (c) 2012
// </copyright>
// <summary>
//   Defines the SharpToCppConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using StyleCop;
    using StyleCop.CSharp;

    public class SharpToCppConverter
    {
        public static bool DoNotSuppressExceptions { get; set; }

        #region Public Methods and Operators

        public static bool ConvertFile(string files, string outputDirectory, string sourceDirectory, string libwinrtdirs)
        {
            return ConvertFile(new[] { files }, outputDirectory, sourceDirectory, libwinrtdirs);
        }

        public static bool ConvertFile(IEnumerable<string> files, string outputDirectory, string sourceDirectory, string libwinrtdirs)
        {
            foreach (var interpreter in GetCsharpInterpretersFromFiles(files, outputDirectory, sourceDirectory, libwinrtdirs))
            {
                interpreter.Save();
            }

            return true;
        }

        public static CodeProject CreateProject(string sourceDirectory)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentException("sourceDirectory");
            }

            var configuration = new Configuration(new string[] { });
            return new CodeProject(sourceDirectory.GetHashCode(), sourceDirectory, configuration);
        }

        public static IEnumerable<SharpToCppInterpreter> GetCsharpInterpretersFromFiles(
            IEnumerable<string> files, string outputDirectory, string sourceDirectory, string libwinrtdirs)
        {
            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new ArgumentException("outputDirectory");
            }

            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentException("sourceDirectory");
            }

            if (string.IsNullOrEmpty(libwinrtdirs))
            {
                throw new ArgumentException("libwinrtdirs");
            }

            FullyQualifiedNamesCache fullyQualifiedNamesCache;
            var csharpDocuments = LoadProject(files, sourceDirectory, libwinrtdirs, out fullyQualifiedNamesCache);
            return GetCsharpInterpretersFromFiles(csharpDocuments, sourceDirectory, outputDirectory, fullyQualifiedNamesCache);
        }

        public static CsDocument ParseCode(string code, CodeProject project)
        {
            var csharpParser = new CsParser();
            var sourceCode = new CodeText(code, project, csharpParser);
            return Parse(sourceCode, project);
        }

        public static CsDocument ParseCsFile(string filePath, CodeProject project)
        {
            var csharpParser = new CsParser();
            var sourceCode = new CodeFile(Path.GetFullPath(filePath), project, csharpParser);
            return Parse(sourceCode, project);
        }

        public static CsDocument ParseHeaderFile(string filePath, CodeProject project)
        {
            return ParseCode(CxxHeaderToCSharpDummyInterpreter.Converter(filePath), project);
        }

        #endregion

        #region Methods

        private static string AdjustOutputDirectory(string absFilePath, string sourceDirectory, string outputDirectory)
        {
            if (!absFilePath.StartsWith(sourceDirectory))
            {
                return Path.GetDirectoryName(absFilePath);
            }

            var sourceDirectoryAbs = Path.GetFullPath(sourceDirectory);
            var subFolder = Path.GetDirectoryName(absFilePath.Substring(sourceDirectoryAbs.Length));
            var directorySeparator = new string(Path.DirectorySeparatorChar, 1);
            var dirSeparator = !outputDirectory.EndsWith(directorySeparator) && !subFolder.StartsWith(directorySeparator)
                                   ? directorySeparator
                                   : string.Empty;
            return string.Concat(outputDirectory, dirSeparator, subFolder);
        }

        private static IEnumerable<SharpToCppInterpreter> GetCsharpInterpretersFromFiles(
            IEnumerable<CsDocument> csharpDocuments,
            string sourceDirectory,
            string outputDirectory,
            FullyQualifiedNamesCache fullyQualifiedNamesCache)
        {
            return from csharpDocument in csharpDocuments
                   let actualOutputDirectory = AdjustOutputDirectory(csharpDocument.SourceCode.Path, sourceDirectory, outputDirectory)
                   let interpreter =
                       new SharpToCppInterpreter(csharpDocument)
                           {
                               OutputDestinationFolder = actualOutputDirectory,
                               FullyQualifiedNames = fullyQualifiedNamesCache
                           }
                   select interpreter;
        }

        private static IEnumerable<CsDocument> LoadProject(
            IEnumerable<string> files, string sourceDirectory, string libwinrtdirs, out FullyQualifiedNamesCache fullyQualifiedNamesCache)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentException("sourceDirectory");
            }

            if (string.IsNullOrEmpty(libwinrtdirs))
            {
                throw new ArgumentException("libwinrtdirs");
            }

            fullyQualifiedNamesCache = new FullyQualifiedNamesCache();

            // fullyQualifiedNamesCache.LoadNamesFrom(@"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0\ExtensionSDKs\Microsoft.VCLibs\11.0\References\CommonConfiguration\neutral\platform.winmd");
            // fullyQualifiedNamesCache.LoadNamesFrom(@"C:\Program Files (x86)\Windows Kits\8.0\References\CommonConfiguration\Neutral\Windows.winmd");
            // todo: create unit test, resolving type of Interop::ElementType should be correct in both cases (if you load platform.winmd or windows.winmd first)
            foreach (var libwinrtdir in libwinrtdirs.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var winmdFilePath in Directory.GetFiles(libwinrtdir, "*.winmd", SearchOption.AllDirectories))
                {
                    fullyQualifiedNamesCache.LoadNamesFrom(winmdFilePath);
                }
            }

            var project = CreateProject(sourceDirectory);

            // include all .h with "ref class"
            foreach (var path in Directory.GetFiles(sourceDirectory, "*.h", SearchOption.AllDirectories))
            {
                var csharpDocument = ParseHeaderFile(path, project);
                if (csharpDocument != null)
                {
                    fullyQualifiedNamesCache.LoadNamesFrom(csharpDocument);
                }
            }

            var documents = new List<CsDocument>();

            // load files which need to be converted
            // get abs file path for each file
            var absFilePathes = new List<string>();
            absFilePathes.AddRange(files.Select(f => Path.GetFullPath(Path.Combine(sourceDirectory, f))));
            
            foreach (var localCsDocument in absFilePathes
                                            .Select(path => ParseCsFile(path, project))
                                            .Where(localCsDocument => localCsDocument != null))
            {
                fullyQualifiedNamesCache.LoadNamesFrom(localCsDocument);
                documents.Add(localCsDocument);
            }

            // load All others Cs files
            foreach (var localCsDocument in Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories)
                                            .Where(path => !absFilePathes.Contains(path))
                                            .Select(path => ParseCsFile(path, project))
                                            .Where(localCsDocument => localCsDocument != null))
            {
                fullyQualifiedNamesCache.LoadNamesFrom(localCsDocument);
            }

            return documents;
        }

        private static CsDocument Parse(SourceCode code, CodeProject project)
        {
            CodeDocument codeDocument = null;

            code.Parser.PreParse();
            try
            {
                var requiredNextPass = code.Parser.ParseFile(code, 0, ref codeDocument);
            }
            catch (ArgumentException)
            {
                if (DoNotSuppressExceptions)
                {
                    throw;
                }
            }
            finally
            {
                code.Parser.PostParse();
            }

            return (CsDocument)codeDocument;
        }

        #endregion
    }
}
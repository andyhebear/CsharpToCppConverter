namespace CsharpToCppConverter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    
    using Converters;

    public class Program
    {
        // /nologo 
        // /outdir:Debug\ 
        // /complist:Source.cs 
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    var exception = eventArgs.ExceptionObject as Exception;
                    if (exception != null)
                    {
                        Debug.Fail(exception.ToString());
                    }
                };

            Execute(ParamsProcessor.ProcessArguments(args));
        }

        internal static void Execute(IDictionary<string, object> arguments)
        {
            Trace.Assert(arguments.Keys.Contains("complist"), "/complist:<file.cs> is not provided");
            if (!arguments.Keys.Contains("complist"))
            {
                return;
            }

            Trace.Assert(arguments.Keys.Contains("srcdir"), "source directory /srcdir:<directory> is not provided");
            if (!arguments.Keys.Contains("srcdir"))
            {
                return;
            }

            Trace.Assert(arguments.Keys.Contains("libwinrtdirs"), ".winmd folder for Platform.winmd and Windows.winmd /libwinrtdirs:<directory;...> is not provided");
            if (!arguments.Keys.Contains("libwinrtdirs"))
            {
                return;
            }

            var srcdir = arguments["srcdir"] as string;
            var libwinrtdirs = arguments["libwinrtdirs"] as string;
            var filePath = arguments["complist"] as string;
            if (filePath != null)
            {
                SharpToCppConverter.ConvertFile(filePath, srcdir, srcdir, libwinrtdirs);
                return;
            }

            var filePaths = arguments["complist"] as IList<string>;
            if (filePaths != null)
            {
                SharpToCppConverter.ConvertFile(filePaths, srcdir, srcdir, libwinrtdirs);
                return;
            }

            throw new ArgumentException("something went wrong");
        }
    }
}

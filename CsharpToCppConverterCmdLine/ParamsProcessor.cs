namespace CsharpToCppConverter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ParamsProcessor
    {
        public static IDictionary<string, object> ProcessArguments(IEnumerable<string> args)
        {
            var arguments = new Dictionary<string, object>();

            foreach (var arg in args)
            {
                string argumentName;
                string argumentValue;
                if (GetArguments(arg, out argumentName, out argumentValue))
                {
                    object value;
                    if (arguments.TryGetValue(argumentName, out value))
                    {
                        var valueAsString = value as string;
                        if (valueAsString != null)
                        {
                            arguments[argumentName] = new List<string> { valueAsString, argumentValue };
                            continue;
                        }

                        var values = value as IList<string>;
                        if (values != null)
                        {
                            values.Add(argumentValue);
                            continue;
                        }

                        throw new ArgumentException("something went wrong");
                    }
                    else
                    {
                        arguments[argumentName] = argumentValue;
                    }
                }
            }

            return arguments;
        }

        public static bool GetArguments(string arg, out string argumentName, out string argumentValue)
        {
            argumentName = string.Empty;
            argumentValue = string.Empty;

            if (!arg.StartsWith("/"))
            {
                return false;
            }

            int colonPosition = arg.IndexOf(':', 1);
            if (colonPosition == -1)
            {
                argumentName = arg.Substring(1, arg.Length - 1);
                return true;
            }

            argumentName = arg.Substring(1, colonPosition - 1);
            argumentValue = arg.Substring(colonPosition + 1, arg.Length - colonPosition - 1);

            return true;
        }
    }
}

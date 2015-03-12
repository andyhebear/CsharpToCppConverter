namespace Converters
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class CxxHeaderToCSharpDummyInterpreter
    {
        public static Regex PropertyFounder = new Regex(@"property([^{}]*){([^{}]*)}\s*\;?");

        public static Regex RedundantColumnFounder = new Regex(@"}\s*;");

        public static Regex RedundantClassDeclarationFounder = new Regex(@"ref\s+class\s+[a-zA-Z_][a-zA-Z_0-9]*\s*(<[^>]*>)?\s*;");

        public static string WhiteSpace = @"\s+";

        public static string WhiteSpaceOpt = @"\s*";
        
        public static string Name = @"[a-zA-Z_][a-zA-Z_0-9]*";
        
        public static string Type = @"((::)?[a-zA-Z_][a-zA-Z_0-9]*)*(<[^>]*>)?\s*(\[[^\]]*\])?\s*[\^\*]?";
        
        public static Regex MethodDeclarationFounder =
            new Regex(string.Format(@"{3}{0}{2}{1}\({1}{3}({1}{2})?({1},{1}{3}({1}{2})?)*{1}\){1};", WhiteSpace, WhiteSpaceOpt, Name, Type));

        public static Regex MethodParametersDeclarationFounder =
            new Regex(string.Format(@"\((({1},)?{1}(?'param'{3}(?'name'{1}{2})?))*{1}\)", WhiteSpace, WhiteSpaceOpt, Name, Type));

        public static string Converter(string cxxHeaderFile)
        {
            var sr = new StreamReader(new FileStream(cxxHeaderFile, FileMode.Open, FileAccess.Read));
            var headerBody = sr.ReadToEnd();
            sr.Close();

            // remove all properties before
            headerBody = Replace(PropertyFounder, headerBody, 2, "get;set;");
            headerBody = ReplaceCustom(MethodParametersDeclarationFounder, headerBody, "param", " p");

            headerBody = RedundantColumnFounder.Replace(headerBody, "}");
            headerBody = RedundantClassDeclarationFounder.Replace(headerBody, string.Empty);

            var csharpDummyClass =
                headerBody.Replace("^", string.Empty).Replace(" ::", " ").Replace("(::", "(").Replace("<::", "<").Replace("[::", "[").
                    Replace("::", ".").Replace("}{}", "}").Replace("property", string.Empty).Replace(
                        "override", string.Empty).Replace("sealed", string.Empty).Replace("ref class", "class").Replace(
                            "internal:", string.Empty).Replace("public:", string.Empty).Replace("public ", string.Empty).Replace(
                                "protected:", string.Empty).Replace("private:", string.Empty).Replace("#", "//#").Replace("(void)", "()").
                    Replace("<void>", string.Empty);

            return csharpDummyClass;
        }

        private static string Replace(Regex regex, string headerBody, int groupNumber, string newValue)
        {
            var positions = new List<KeyValuePair<int, int>>();
            var match = regex.Match(headerBody);
            while (match.Success)
            {
                var @group = match.Groups[groupNumber];
                foreach (Capture capture in @group.Captures)
                {
                    positions.Insert(0, new KeyValuePair<int, int>(capture.Index, capture.Length));
                }

                match = match.NextMatch();
            }

            foreach (var position in positions)
            {
                headerBody = headerBody.Remove(position.Key, position.Value);
                headerBody = headerBody.Insert(position.Key, newValue);
            }

            return headerBody;
        }

        private static string ReplaceCustom(Regex regex, string headerBody, string groupName, string newValue)
        {
            var positions = new List<KeyValuePair<int, int>>();
            var match = regex.Match(headerBody);
            while (match.Success)
            {
                var @group = match.Groups[groupName];
                foreach (Capture capture in @group.Captures)
                {
                    positions.Insert(0, new KeyValuePair<int, int>(capture.Index, capture.Length));
                }

                match = match.NextMatch();
            }

            int number = 0;
            foreach (var position in positions)
            {
                var subString = headerBody.Substring(position.Key, position.Value);
                var index = subString.LastIndexOf(' ');
                if (index == -1 && !string.IsNullOrWhiteSpace(subString))
                {
                    headerBody = headerBody.Remove(position.Key, position.Value);
                    headerBody = headerBody.Insert(position.Key, string.Concat(subString, newValue, number));
                }

                number++;
            }

            return headerBody;
        }
    }
}

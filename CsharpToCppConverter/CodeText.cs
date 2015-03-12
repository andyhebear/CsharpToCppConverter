namespace Converters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using StyleCop;

    public class CodeText : SourceCode
    {
        #region Constants and Fields

        private readonly string text;

        #endregion

        #region Constructors and Destructors

        public CodeText(string text, CodeProject project, SourceParser parser)
            : this(text, project, parser, null)
        {
            Param.Ignore(text, project, parser);
        }

        public CodeText(string text, CodeProject project, SourceParser parser, IEnumerable<Configuration> configurations)
            : base(project, parser, configurations)
        {
            Param.RequireNotNull(text, "text");
            Param.RequireNotNull(project, "project");
            Param.RequireNotNull(parser, "parser");
            Param.Ignore(configurations);

            this.text = text;
        }

        #endregion

        #region Public Properties

        public override bool Exists
        {
            get
            {
                return true;
            }
        }

        public string Folder
        {
            get
            {
                return string.Empty;
            }
        }

        public string FullPathName
        {
            get
            {
                return string.Empty;
            }
        }

        public override string Name
        {
            get
            {
                return string.Empty;
            }
        }

        public override string Path
        {
            get
            {
                return string.Empty;
            }
        }

        public override DateTime TimeStamp
        {
            get
            {
                return new DateTime();
            }
        }

        public override string Type
        {
            get
            {
                return "cs";
            }
        }

        #endregion

        #region Public Methods and Operators

        public override TextReader Read()
        {
            return new StringReader(this.text);
        }

        #endregion
    }
}

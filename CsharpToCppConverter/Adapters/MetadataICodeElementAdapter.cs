namespace Converters.Metadata
{
    using System;
    using System.Collections.Generic;

    using StyleCop;

    public class MetadataICodeElementAdapter : ICodeElement
    {
        private IList<Violation> violations;

        public MetadataICodeElementAdapter(MetadataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            this.Reader = reader;
        }

        public MetadataReader Reader { get; protected set; }

        public IList<Violation> Violations
        {
            get
            {
                return this.violations ?? (this.violations = new List<Violation>());
            }
        }

        public bool AddViolation(Violation violation)
        {
            this.Violations.Add(violation);
            return true;
        }

        public virtual IEnumerable<ICodeElement> ChildCodeElements
        {
            get
            {
                yield break;
            }
        }

        public void ClearAnalyzerTags()
        {
        }

        public CodeDocument Document
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string FullyQualifiedName
        {
            get
            {
                return "Root";
            }
        }

        public int LineNumber
        {
            get { throw new NotImplementedException(); }
        }

        ICollection<Violation> ICodeElement.Violations
        {
            get { return this.Violations; }
        }

        public virtual bool IsNamespace
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsFieldOrVariable 
        { 
            get 
            {
                return false;
            }  
        }

        public virtual bool IsClassName 
        { 
            get 
            {
                return false;
            } 
        }

        public virtual bool IsStatic
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsValueType
        {
            get
            {
                return false;
            }
        }
    }
}

using System;

namespace HotGlue
{
    public class FileReference
    {
        private String name;

        public String Name
        {
            get { return name; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    name = "";
                }
                else
                {
                    name = value.Trim();
                }
            }
        }

        private String variable;

        public String Variable
        {
            get { return variable; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    variable = "";
                }
                else
                {
                    variable = value.Trim();
                }
            }
        }

        public String Path { get; set; }

        public Boolean Wrap { get; set; }

        public override bool Equals(object obj)
        {
            var reference = obj as FileReference;
            if (reference == null) return false;

            return String.Compare(name, reference.name) == 0 &&
                   String.Compare(variable, reference.variable) == 0 &&
                   String.Compare(Path, reference.Path) == 0 &&
                   Wrap == reference.Wrap;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 11;
                hash = hash * 7 + name.GetHashCode();
                hash = hash * 7 + variable.GetHashCode();
                hash = hash * 7 + Path.GetHashCode();
                hash = hash * 7 + Wrap.GetHashCode();
                return hash;
            }
        }
    }
}
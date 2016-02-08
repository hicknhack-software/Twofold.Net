using System;

namespace Twofold.Api.SourceMapping
{
    public class TextFilePosition : TextPosition, IEquatable<TextFilePosition>
    {
        public readonly string Name;

        public TextFilePosition(string name)
        {
            Name = name;
        }

        public TextFilePosition(string name, TextPosition textPosition)
            : base(textPosition.Line, textPosition.Column)
        {
            Name = name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Name} L:{Line}, C:{Column}";
        }

        #region IEquatable
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((TextPosition)obj);
        }

        public bool Equals(TextFilePosition other)
        {
            if (other == null) {
                return false;
            }
            return base.Equals((TextPosition)other)
                && (string.Compare(Name, other.Name) == 0)
                ;
        }
        #endregion
    }
}

using System;

namespace Twofold.Api.SourceMapping
{
    public class TextFilePosition : TextPosition, IEquatable<TextFilePosition>
    {
        public readonly string SourceName;

        public TextFilePosition(string sourceName)
        {
            SourceName = sourceName;
        }

        public TextFilePosition(string sourceName, TextPosition textPosition)
            : base(textPosition.Line, textPosition.Column)
        {
            SourceName = sourceName;
        }

        public override int GetHashCode()
        {
            return SourceName.GetHashCode() ^ base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{SourceName} L:{Line}, C:{Column}";
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
                && (string.Compare(SourceName, other.SourceName) == 0)
                ;
        }
        #endregion
    }
}

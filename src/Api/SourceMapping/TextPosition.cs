using System;

namespace Twofold.Api.SourceMapping
{
    public class TextPosition : IEquatable<TextPosition>
    {
        int line = 0;
        int column = 0;

        public bool IsValid
        {
            get { return Line != 0 && Column != 0; }
        }
        public int Line
        {
            get { return line; }
            set
            {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("Line", "Must be greater than zero.");
                }
                line = value;
            }
        }
        public int Column
        {
            get { return column; }
            set
            {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("Column", "Must be greater than zero.");
                }
                column = value;
            }
        }

        /// <summary>
        /// Creates an invalid TextPosition.
        /// </summary>
        public TextPosition()
        { }

        /// <summary>
        /// Creates a TextPosition.
        /// </summary>
        public TextPosition(int line, int column)
        {
            if (line < 1) {
                throw new ArgumentOutOfRangeException("line", "Must be greater than zero.");
            }
            if (column < 1) {
                throw new ArgumentOutOfRangeException("column", "Must be greater than zero.");
            }
            Line = line;
            Column = column;
        }

        public override int GetHashCode()
        {
            return Line.GetHashCode() ^ Column.GetHashCode();
        }

        public override string ToString()
        {
            return $"L:{Line}, C:{Column}";
        }

        #region IEquatable
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((TextPosition)obj);
        }

        public bool Equals(TextPosition other)
        {
            if (other == null) {
                return false;
            }
            return Line == other.Line
                && Column == other.Column
                ;
        }
        #endregion
    }
}

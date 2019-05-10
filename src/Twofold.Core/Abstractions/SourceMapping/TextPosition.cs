﻿/* Twofold.Net
 * (C) Copyright 2016 HicknHack Software GmbH
 *
 * The original code can be found at:
 *     https://github.com/hicknhack-software/Twofold.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace HicknHack.Twofold.Abstractions.SourceMapping
{
    using System;

    public class TextPosition : IEquatable<TextPosition>, IComparable<TextPosition>
    {
        private int line = 0;
        private int column = 0;

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
            if (line < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "Must be greater than zero.");
            }
            if (column < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "Must be greater than zero.");
            }
            this.Line = line;
            this.Column = column;
        }

        /// <summary>
        /// Creates a TextPosition.
        /// </summary>
        /// <param name="textPosition">The source TextPosition.</param>
        public TextPosition(TextPosition textPosition)
        {
            this.line = textPosition.line;
            this.column = textPosition.column;
        }

        public bool IsValid
        {
            get { return (this.line != 0) && (this.column != 0); }
        }

        public int Line
        {
            get { return this.line; }
            private set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Line), "Must be greater than zero.");
                }
                this.line = value;
            }
        }

        public int Column
        {
            get { return this.column; }
            private set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Column), "Must be greater than zero.");
                }
                this.column = value;
            }
        }

        public override int GetHashCode()
        {
            return this.Line.GetHashCode()
                ^ this.Column.GetHashCode();
        }

        public override string ToString()
        {
            return $"({this.line}, {this.column})";
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
            if (other == null)
            {
                return false;
            }
            return this.line == other.line
                && this.column == other.column
                ;
        }

        #endregion IEquatable

        #region IComparable

        public int CompareTo(TextPosition other)
        {
            int lineDiff = (this.line - other.line);
            if (0 != lineDiff)
            {
                return lineDiff;
            }

            int columnDiff = (this.column - other.column);
            if (0 != columnDiff)
            {
                return columnDiff;
            }

            return 0;
        }

        #endregion IComparable
    }
}
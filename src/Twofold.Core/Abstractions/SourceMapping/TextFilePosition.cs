/* Twofold.Net
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

    public class TextFilePosition : TextPosition, IEquatable<TextFilePosition>
    {
        public readonly string Name;

        public TextFilePosition()
        { }

        public TextFilePosition(string name, int line, int column)
            : this(name, new TextPosition(line, column))
        { }

        public TextFilePosition(string name, TextPosition textPosition)
            : base(textPosition)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public new bool IsValid
        {
            get
            {
                return string.IsNullOrEmpty(this.Name) == false
                    && base.IsValid;
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Name}: {base.ToString()}";
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
            if (other == null)
            {
                return false;
            }
            return base.Equals((TextPosition)other)
                && (string.Compare(this.Name, other.Name, StringComparison.Ordinal) == 0)
                ;
        }

        #endregion IEquatable
    }
}
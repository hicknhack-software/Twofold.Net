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

using System;

namespace HicknHack.Twofold.Abstractions.SourceMapping
{
    public class MappingEntry
    {
        public readonly TextPosition Generated;
        public readonly TextFilePosition Source;
        public readonly int CallerIndex;
        public readonly EntryFeatures Features;

        public MappingEntry()
            : this(new TextPosition(), new TextFilePosition(), -1, EntryFeatures.None)
        { }

        public MappingEntry(TextPosition generated, TextFilePosition original, int callerIndex, EntryFeatures features)
        {
            this.Generated = generated ?? throw new ArgumentNullException(nameof(generated));
            this.Source = original ?? throw new ArgumentNullException(nameof(original));

            this.CallerIndex = callerIndex;
            this.Features = features;
        }

        public bool IsValid
        {
            get { return this.Generated.IsValid; }
        }

        public override string ToString()
        {
            return $"{this.Generated.ToString()}, {this.Source.ToString()} [CallerIndex: {this.CallerIndex}]";
        }
    }
}
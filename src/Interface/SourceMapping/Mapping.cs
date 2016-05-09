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

namespace Twofold.Interface.SourceMapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Mapping
    {
        public class Caller
        {
            public readonly TextFilePosition Source;
            public readonly int ParentIndex;

            public Caller(TextFilePosition source, int parentIndex)
            {
                this.Source = source;
                this.ParentIndex = parentIndex;
            }

            public override string ToString()
            {
                return $"{this.ParentIndex} <- {this.Source.ToString()}";
            }
        }

        private List<MappingEntry> Mappings = new List<MappingEntry>();
        private List<Caller> Callers = new List<Caller>();

        public List<TextFilePosition> CallerStack(TextPosition generated)
        {
            if (generated == null)
            {
                throw new ArgumentNullException(nameof(generated));
            }

            if (generated.IsValid == false)
            {
                throw new ArgumentException("Is invalid.", nameof(generated));
            }

            var callerStack = new List<TextFilePosition>();
            MappingEntry entry = this.FindEntryByGenerated(generated);
            if (entry.IsValid == false)
            {
                return callerStack;
            }

            callerStack.Add(entry.Source);
            int callerIndex = entry.CallerIndex;
            while (callerIndex != -1)
            {
                Caller caller = this.Callers[callerIndex];
                callerStack.Add(caller.Source);
                callerIndex = caller.ParentIndex;
            }
            return callerStack;
        }

        public TextFilePosition FindSourceByGenerated(TextPosition generated)
        {
            if (generated == null)
            {
                throw new ArgumentNullException(nameof(generated));
            }

            if (generated.IsValid == false)
            {
                throw new ArgumentException("Is invalid.", nameof(generated));
            }

            MappingEntry entry = this.FindEntryByGenerated(generated);
            if (entry.IsValid == false)
            {
                return new TextFilePosition();
            }

            if ((entry.Features & EntryFeatures.ColumnInterpolation) == 0)
            {
                return entry.Source;
            }

            int line = (entry.Generated.Line - generated.Line) + entry.Source.Line;
            int column = (entry.Generated.Column - generated.Column) + entry.Source.Column;
            return new TextFilePosition(entry.Source.SourceName, line, column);
        }

        public void Add(MappingEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            this.Mappings.Add(entry);
        }

        public int AddCaller(TextFilePosition source, int parentIndex)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Callers.Add(new Caller(source, parentIndex));
            return (this.Callers.Count - 1);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("//Mappings");
            foreach (var mapping in this.Mappings)
            {
                sb.AppendLine(mapping.ToString());
            }
            sb.AppendLine();

            sb.AppendLine("//Callers");
            int callerIndex = 0;
            foreach (var caller in this.Callers)
            {
                sb.Append($"{callerIndex}: ");
                sb.AppendLine(caller.ToString());
                ++callerIndex;
            }

            return sb.ToString();
        }

        private MappingEntry FindEntryByGenerated(TextPosition generated) => this.Mappings.LastOrDefault(e => e.Generated.CompareTo(generated) <= 0);
    }
}
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
    }
}
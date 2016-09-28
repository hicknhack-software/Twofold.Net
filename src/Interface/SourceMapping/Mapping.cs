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
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
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

            int line = Math.Abs(entry.Generated.Line - generated.Line) + entry.Source.Line;
            int column = Math.Abs(entry.Generated.Column - generated.Column) + entry.Source.Column;
            return new TextFilePosition(entry.Source.Name, line, column);
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

        public void Write(Stream stream, string generatedFilePath)
        {
            this.Write(stream, generatedFilePath, string.Empty);
        }

        public void Write(Stream stream, string generatedFilePath, string sourceFilePathRoot)
        {
            if(string.IsNullOrEmpty(sourceFilePathRoot) == false && 
                (sourceFilePathRoot.EndsWith("\\") || sourceFilePathRoot.EndsWith("/")) == false)
            {
                sourceFilePathRoot = sourceFilePathRoot + Path.DirectorySeparatorChar;
            }

            // Gather sources
            var sources = new List<string>();
            var sourcesIndex = new Dictionary<string, int>();
            foreach (var mappingEntry in Mappings)
            {
                string filepath = AbsolutePath(mappingEntry.Source.Name);
                if (sourcesIndex.ContainsKey(filepath))
                {
                    continue;
                }

                sourcesIndex.Add(filepath, sources.Count);
                string relativeFilePath = mappingEntry.Source.Name;
                if (string.IsNullOrEmpty(sourceFilePathRoot) == false)
                {
                    var rootUri = new UriBuilder(sourceFilePathRoot).Uri;
                    //Note(Maik): Path.MakeRelativeUri() requires the presence of a trailing slash in the
                    // 'this' URI.
                    relativeFilePath = rootUri.MakeRelativeUri(new UriBuilder(mappingEntry.Source.Name).Uri).ToString();
                }
                sources.Add(relativeFilePath);
            }

            // Build mappings
            var mappings = new StringBuilder();
            var indices = new StringBuilder();
            var columnInterpolation = new StringBuilder();

            bool newLine = true;
            int prevSourceLine = 1;
            int prevSourceColumn = 1;

            int prevGeneratedLine = 1;
            int prevGeneratedColumn = 1;

            int prevSourcesIndex = 0;

            int prevCallerIndex = 0;

            foreach (var mappingEntry in Mappings)
            {
                int lineDiff = (mappingEntry.Generated.Line - prevGeneratedLine);
                for (int i = 0; i < lineDiff; ++i)
                {
                    mappings.Append(";");
                    indices.Append(";");
                    columnInterpolation.Append(";");
                }
                if (lineDiff > 0)
                {
                    prevGeneratedLine = mappingEntry.Generated.Line;
                    prevGeneratedColumn = 1;
                    newLine = true;
                }

                if (lineDiff == 0 && newLine == false)
                {
                    mappings.Append(",");
                    indices.Append(",");
                    columnInterpolation.Append(",");
                }

                // Field 1: "zero-based starting column of the line in the generated code"
                int generatedColumn = mappingEntry.Generated.Column;
                VLQ.Encode(mappings, generatedColumn - prevGeneratedColumn);
                prevGeneratedColumn = generatedColumn;

                // Field 2: "zero-based index into the sources list"
                int sourceIndex = sourcesIndex[AbsolutePath(mappingEntry.Source.Name)];
                VLQ.Encode(mappings, sourceIndex - prevSourcesIndex);
                prevSourcesIndex = sourceIndex;

                // Field 3: "zero-based starting line in the source"
                int sourceLine = mappingEntry.Source.Line;
                VLQ.Encode(mappings, sourceLine - prevSourceLine);
                prevSourceLine = sourceLine;

                // Field 4: "zero-based starting column of the line in the source"
                int sourceColumn = mappingEntry.Source.Column;
                VLQ.Encode(mappings, sourceColumn - prevSourceColumn);
                prevSourceColumn = sourceColumn;

                //
                //
                int callerIndex = mappingEntry.CallerIndex;
                if (callerIndex != -1)
                {
                    VLQ.Encode(indices, callerIndex - prevCallerIndex);
                    prevCallerIndex = callerIndex;
                }

                //
                //
                if ((mappingEntry.Features & EntryFeatures.ColumnInterpolation) != 0)
                {
                    VLQ.Encode(columnInterpolation, 1);
                }

                newLine = false;
            }

            // Build Callstack
            var callers = new StringBuilder();
            prevSourcesIndex = 0;
            prevSourceLine = 1;
            prevSourceColumn = 1;
            int prevParentIndex = 0;
            foreach (var caller in Callers)
            {
                if (callers.Length > 0)
                {
                    callers.Append(";");
                }

                // Field 1: "zero-based index into the sources list"
                int sourceIndex = sourcesIndex[AbsolutePath(caller.Source.Name)];
                VLQ.Encode(callers, sourceIndex - prevSourcesIndex);
                prevSourcesIndex = sourceIndex;

                // Field 2: "zero-based starting line in the source"
                int sourceLine = caller.Source.Line;
                VLQ.Encode(callers, sourceLine - prevSourceLine);
                prevSourceLine = sourceLine;

                // Field 3: "zero-based starting column of the line in the source"
                int sourceColumn = caller.Source.Column;
                VLQ.Encode(callers, sourceColumn - prevSourceColumn);
                prevSourceColumn = sourceColumn;

                // Field 4: "zero-based index of parent caller"
                int parentIndex = caller.ParentIndex;
                if (parentIndex != -1)
                {
                    VLQ.Encode(callers, parentIndex - prevParentIndex);
                    prevParentIndex = parentIndex;
                }
            }

            var callstack = new CallstackExtension { Callers = callers.ToString(), Indices = indices.ToString() };
            var graph = new SourceMapGraph
            {
                File = new UriBuilder(generatedFilePath).Path,
                SourceRoot = string.IsNullOrEmpty(sourceFilePathRoot) ? string.Empty : new UriBuilder(sourceFilePathRoot).Path,
                Sources = sources,
                Mappings = mappings.ToString(),
                Callstack = callstack,
                ColumnInterpolation = columnInterpolation.ToString()
            };
            var serializer = new DataContractJsonSerializer(typeof(SourceMapGraph));
            serializer.WriteObject(stream, graph);
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

        [DataContract]
        public class SourceMapGraph
        {
            [DataMember(Name = "version", Order = 0)]
            public int Version = 3;

            [DataMember(Name = "file", Order = 1)]
            public string File = string.Empty;

            [DataMember(Name = "sourceRoot", Order = 2)]
            public string SourceRoot = string.Empty;

            [DataMember(Name = "sources", Order = 3)]
            public List<string> Sources = new List<string> { };

            [DataMember(Name = "sourcesContent", Order = 4)]
            public List<Object> SourcesContent = new List<Object> { };

            [DataMember(Name = "names", Order = 5)]
            public List<string> Names = new List<string> { };

            [DataMember(Name = "mappings", Order = 6)]
            public string Mappings = string.Empty;

            [DataMember(Name = "x_de_hicknhack_software_column_interpolation", Order = 7)]
            public string ColumnInterpolation = string.Empty;

            [DataMember(Name = "x_de_hicknhack_software_callstack", Order = 8)]
            public CallstackExtension Callstack = new CallstackExtension();
        }

        [DataContract]
        public class CallstackExtension
        {
            [DataMember(Name = "callers", Order = 0)]
            public string Callers = string.Empty;

            [DataMember(Name = "indices", Order = 1)]
            public string Indices = string.Empty;
        }

        private string AbsolutePath(string path) => new UriBuilder(path).Path;
    }
}
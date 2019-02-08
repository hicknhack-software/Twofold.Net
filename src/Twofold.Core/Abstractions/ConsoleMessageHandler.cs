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

namespace HicknHack.Twofold.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using SourceMapping;

    public class ConsoleMessageHandler : IMessageHandler
    {
        public void Message(TraceLevel level, string text, string file, TextPosition position)
        {
            string file_ = file ?? string.Empty;

            string position_ = string.Empty;
            var positionBuilder = new StringBuilder();
            if (position.IsValid)
            {
                positionBuilder
                    .Append("(")
                    .Append(position.Line).Append(",").Append(position.Column)
                    .Append(")");
            }
            position_ = positionBuilder.ToString();

            string level_ = string.Empty;
            switch (level)
            {
                case TraceLevel.Verbose:
                    level_ = "VER";
                    break;
                case TraceLevel.Info:
                    level_ = "INF";
                    break;
                case TraceLevel.Warning:
                    level_ = "WRN";
                    break;
                case TraceLevel.Error:
                    level_ = "ERR";
                    break;
                default:
                    Debug.Assert(false, $"Unhandled case: {level}");
                    break;
            }

            string text_ = text ?? string.Empty;

            Console.Error.WriteLine($"{file_}{position_}: {level_}: {text_}");
        }

        public void StackTrace(List<StackFrame> frames)
        {
            var sb = new StringBuilder();
            foreach (StackFrame frame in frames)
            {
                sb.Clear();
                sb.Append(' ', 4).Append("at ").Append(frame.Method ?? string.Empty);
                if (string.IsNullOrEmpty(frame.File) == false)
                {
                    sb.Append(" in ").Append(frame.File);
                    if (frame.Line.HasValue)
                    {
                        sb.Append(" (").Append(frame.Line);
                        if (frame.Column.HasValue)
                        {
                            sb.Append(",").Append(frame.Column);
                        }

                        sb.Append(")");
                    }
                }

                Console.Error.WriteLine(sb.ToString());
            }
        }
    }
}

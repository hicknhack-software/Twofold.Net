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
    public class StackFrame
    {
        public StackFrame(string method) : this(method, null, null, null)
        { }

        public StackFrame(string method, string file) : this(method, file, null, null)
        { }

        public StackFrame(string method, string file, int line) : this(method, file, line, null)
        { }

        public StackFrame(string method, string file, int? line, int? column)
        {
            this.Method = method;
            this.File = file;
            this.Line = line;
            this.Column = column;
        }

        public string Method { get; private set; }
        public string File { get; private set; }
        public int? Line { get; private set; }
        public int? Column { get; private set; }
    }
}

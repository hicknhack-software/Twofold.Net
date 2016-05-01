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

namespace Twofold.Interface
{
    using SourceMapping;
    using System.Diagnostics;

    public interface IMessageHandler
    {
        /// <summary>
        /// Invoke if a message occured while executing Twofold.
        /// </summary>
        /// <param name="level">Severity of the message.</param>
        /// <param name="text">Message text.</param>
        /// <param name="path">Source, e.g. filepath, the message belongs to. Can be empty.</param>
        /// <param name="position">Position the message belongs to.</param>
        void Message(TraceLevel level, string text, string path, TextPosition position);
    };
}
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
    using System.Text;

    public static class VLQ
    {
        private const int BaseShift = 5;
        private const int Base = (1 << BaseShift);
        private const int BaseMask = (Base - 1);
        private const int ContinuationBit = Base;
        private const string Base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public static void Encode(StringBuilder sb, int value)
        {
            int vlq = VLQ.ToVLQSigned(value);
            do
            {
                int digit = (vlq & VLQ.BaseMask);
                vlq >>= VLQ.BaseShift;
                if (vlq > 0)
                {
                    digit |= VLQ.ContinuationBit;
                }
                sb.Append(VLQ.Base64Chars[digit]);
            }
            while (vlq > 0);
        }

        private static int ToVLQSigned(int value)
        {
            return (value < 0) ?
                ((-value) << 1) + 1
                :
                (value << 1) + 0;
        }
    }
}

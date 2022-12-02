//-----------------------------------------------------------------------------
// FILE:        FixedLengthStream.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Tasks;

namespace Neon.IO
{
    /// <summary>
    /// <para>
    /// Implements a specialized stream that wraps another stream that does not support
    /// seeking such that the <see cref="Length"/> property will return a specific value.
    /// </para>
    /// <para>
    /// This is useful for situations such as a web request handler that needs to process
    /// the body stream which does not implement <see cref="Length"/> but where this length
    /// is required.
    /// </para>
    /// <para>
    /// To use, simply construct an instance, passing the source stream and its length
    /// (often obtained via an HTTP <b>Content-Length</b> header.
    /// </para>
    /// <note>
    /// <para>
    /// This stream is really intended just for reading data and does not support:
    /// </para>
    /// </note>
    /// <list type="bullet">
    /// <item>writing</item>
    /// <item>seeking</item>
    /// <item>setting the length</item>
    /// </list>
    /// </summary>
    public class FixedLengthStream : Stream
    {
        private Stream  stream;
        private long    length;
        private long    position;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="length">The stream length.</param>
        public FixedLengthStream(Stream stream, long length)
        {
            Covenant.Requires<ArgumentNullException>(stream != null, nameof(stream));
            Covenant.Requires<ArgumentException>(stream.CanRead, nameof(stream));
            Covenant.Requires<ArgumentException>(length >= 0, nameof(length));

            this.stream   = stream;
            this.length   = length;
            this.position = 0;
        }

        /// <inheritdoc/>
        public override bool CanRead => stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => true;   // Returning [true] here to indicate that [Length] will work.  Seek operations will fail though.

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <summary>
        /// <para>
        /// Returns the fixed stream length passed to the constructor.
        /// </para>
        /// </summary>
        public override long Length => length;

        /// <inheritdoc/>
        public override long Position { get => position; set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = stream.Read(buffer, offset, count);
            position += bytesRead;

            return bytesRead;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}

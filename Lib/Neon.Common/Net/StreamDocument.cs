//-----------------------------------------------------------------------------
// FILE:	    StreamDocument.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Neon.Common;
using Neon.Collections;
using Neon.Diagnostics;
using Neon.ModelGen;
using Neon.Retry;
using Neon.Tasks;

namespace Neon.Net
{
    /// <summary>
    /// <para>
    /// Used by <b>ModelGen</b> generated service client methods to upload body data
    /// from a stream as opposed to serializing a document as JSON for service model
    /// methods tagged with <see cref="BodyStreamAttribute"/>.
    /// </para>
    /// <para>
    /// <see cref="StreamDocument"/> instances may be passed as the <c>document</c>
    /// parameter to <see cref="JsonClient"/> methods which recognizes these documents
    /// as special by uploading the stream data instead of JSON.
    /// </para>
    /// <note>
    /// You may also use special document directly in your code if necessary.
    /// </note>
    /// </summary>
    public class StreamDocument
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">
        /// The stream whose contents from the current position to the end of the 
        /// stream will be uploaded as the request body.
        /// </param>
        public StreamDocument(Stream stream)
        {
            Covenant.Requires<ArgumentNullException>(stream != null, nameof(stream));

            this.Stream = stream;
        }

        /// <summary>
        /// Returns the stream whose contents from the current position to the end of the 
        /// stream will be uploaded as the request body.
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// Specifies the <b>Content-Type</b> to be used for the uploaded data. 
        /// This default to <b>application/octet-stream</b>.
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// Specifies the size of the buffer to be used for transmitting the
        /// body data.  This defaults to <b>8 GiB</b>.
        /// </summary>
        public int BufferSize { get; set; } = 8192;
    }
}

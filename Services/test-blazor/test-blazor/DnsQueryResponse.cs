// -----------------------------------------------------------------------------
// FILE:	    DnsQueryResponse.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Linq;

using DnsClient;
using DnsClient.Protocol;

namespace TestBlazor
{
    internal class DnsQueryResponse : IDnsQueryResponse
    {
        private int? _hashCode;

        public NameServer NameServer { get; }

        public IReadOnlyList<DnsResourceRecord> Additionals { get; }

        public IEnumerable<DnsResourceRecord> AllRecords => Answers.Concat(Additionals).Concat(Authorities);

        public string AuditTrail { get; private set; }

        public IReadOnlyList<DnsResourceRecord> Answers { get; set; }

        public IReadOnlyList<DnsResourceRecord> Authorities { get; }

        public string ErrorMessage => string.Empty;

        public bool HasError => false;

        public DnsResponseHeader Header { get; }

        public IReadOnlyList<DnsQuestion> Questions { get; }

        public int MessageSize { get; }

        public DnsQuerySettings Settings { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is DnsQueryResponse dnsQueryResponse))
            {
                return false;
            }

            if (Header.ToString().Equals(dnsQueryResponse.Header.ToString(), StringComparison.OrdinalIgnoreCase) && string.Join("", Questions).Equals(string.Join("", dnsQueryResponse.Questions), StringComparison.OrdinalIgnoreCase))
            {
                return string.Join("", AllRecords).Equals(string.Join("", dnsQueryResponse.AllRecords), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                string text = Header.ToString() + string.Join("", Questions) + string.Join("", AllRecords);
                _hashCode = text.GetHashCode(StringComparison.Ordinal);
            }

            return _hashCode.Value;
        }

        internal static void SetAuditTrail(IDnsQueryResponse response, string value)
        {
            if (response is DnsQueryResponse dnsQueryResponse)
            {
                dnsQueryResponse.AuditTrail = value;
            }
        }
    }
}

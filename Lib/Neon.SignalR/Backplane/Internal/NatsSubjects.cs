//-----------------------------------------------------------------------------
// FILE:        NatsSubjects.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

namespace Neon.SignalR
{
    internal class NatsSubjects
    {
        private readonly string prefix;

        /// <summary>
        /// Gets the name of the subject for sending to all connections.
        /// </summary>
        public string All { get; }

        /// <summary>
        /// Gets the name of the internal subject for group management messages.
        /// </summary>
        public string GroupManagement { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prefix">The subject prefix.</param>
        public NatsSubjects(string prefix)
        {
            this.prefix = prefix;

            All             = prefix + ".all";
            GroupManagement = prefix + ".internal.groups";
        }

        /// <summary>
        /// Gets the name of the subject for sending a message to a specific connection.
        /// </summary>
        /// <param name="connectionId">The ID of the connection to get the subject for.</param>
        public string Connection(string connectionId)
        {
            return prefix + ".connection." + connectionId;
        }

        /// <summary>
        /// Gets the name of the subject for sending a message to a named group of connections.
        /// </summary>
        /// <param name="groupName">The name of the group to get the subject for.</param>
        public string Group(string groupName)
        {
            return prefix + ".group." + groupName;
        }

        /// <summary>
        /// Gets the name of the subject for sending a message to all collections associated with a user.
        /// </summary>
        /// <param name="userId">The ID of the user to get the subject for.</param>
        public string User(string userId)
        {
            return prefix + ".user." + userId;
        }

        /// <summary>
        /// Gets the name of the acknowledgement subject for the specified server.
        /// </summary>
        /// <param name="serverName">The name of the server to get the acknowledgement subject for.</param>
        /// <returns></returns>
        public string Ack(string serverName)
        {
            return prefix + ".internal.ack." + serverName;
        }

        /// <summary>
        /// Gets the name of the client return results subject for the specified server.
        /// </summary>
        /// <param name="serverName">The name of the server to get the client return results subject for.</param>
        /// <returns></returns>
        public string ReturnResults(string serverName)
        {
            return prefix + ".internal.return." + serverName;
        }
    }
}

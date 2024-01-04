//-----------------------------------------------------------------------------
// FILE:        GroupCommand.cs
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;

using MessagePack;

using Newtonsoft.Json;

namespace Neon.Web.SignalR
{
    /// <summary>
    /// Used to serialize a group command.
    /// </summary>
    [MessagePackObject]
    public class GroupCommand
    {
        /// <summary>
        /// The ID of the group command.
        /// </summary>
        [Key(0)]
        public int Id { get; set; }

        /// <summary>
        /// The name of the server that sent the command.
        /// </summary>
        [Key(1)]
        [DefaultValue(null)]
        public string ServerName { get; set; } = null;

        /// <summary>
        /// The action to be performed on the group.
        /// </summary>
        [Key(2)]
        public GroupAction Action { get; set; }

        /// <summary>
        /// Gets the group on which the action is performed.
        /// </summary>
        [Key(3)]
        public string GroupName { get; set; } = null;

        /// <summary>
        /// Gets the ID of the connection to be added or removed from the group.
        /// </summary>
        [Key(4)]
        public string ConnectionId { get; set; } = null;

        /// <summary>
        /// Writes a <see cref="GroupCommand"/> to a byte array.
        /// </summary>
        /// <param name="id">The message ID.</param>
        /// <param name="serverName">The target server.</param>
        /// <param name="action">The action.</param>
        /// <param name="groupName">The group name.</param>
        /// <param name="connectionId">The connection ID.</param>
        /// <returns></returns>
        public static byte[] Write(int id, string serverName, GroupAction action, string groupName, string connectionId)
        {
            var command = new GroupCommand()
            {
                Id           = id,
                ServerName   = serverName,
                Action       = action,
                GroupName    = groupName,
                ConnectionId = connectionId
            };

            return MessagePackSerializer.Serialize(command);
        }

        /// <summary>
        /// Reads an <see cref="GroupCommand"/> from a byte array.
        /// </summary>
        /// <param name="message">The message bytes.</param>
        /// <returns>The parsed <see cref="GroupCommand"/>.</returns>
        public static GroupCommand Read(byte[] message)
        {
            return MessagePackSerializer.Deserialize<GroupCommand>(message);
        }
    }
}

//-----------------------------------------------------------------------------
// FILE:        PortalBinder.cs
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

using System.Collections.Generic;

namespace Neon.Tailwind
{
    public class PortalBinder : IPortalBinder
    {
        private Dictionary<string, Portal> portals = new Dictionary<string, Portal>();
        public void RegisterPortal(string name, Portal portal)
        {
            portals[name] = portal;
        }
        public void UnRegisterPortal(string name, Portal portal)
        {
            if (portals.ContainsKey(name))
            {
                portals.Remove(name);
            }
        }

        public Portal GetPortal(string name)
        {
            if (portals.TryGetValue(name, out var portal))
            {
                return portal;
            }
            return null;
        }
    }
}

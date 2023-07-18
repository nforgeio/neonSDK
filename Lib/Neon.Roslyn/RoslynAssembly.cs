//-----------------------------------------------------------------------------
// FILE:        RoslynAssembly.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    internal class RoslynAssembly : Assembly
    {
        private readonly MetadataLoadContext _metadataLoadContext;

        public RoslynAssembly(IAssemblySymbol assembly, MetadataLoadContext metadataLoadContext)
        {
            Symbol               = assembly;
            _metadataLoadContext = metadataLoadContext;
        }

        public override string FullName => Symbol.Name;

        internal IAssemblySymbol Symbol { get; }

        public override Type[] GetExportedTypes()
        {
            return GetTypes();
        }

        public override Type[] GetTypes()
        {
            var types = new List<Type>();
            var stack = new Stack<INamespaceSymbol>();

            stack.Push(Symbol.GlobalNamespace);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                foreach (var type in current.GetTypeMembers())
                {
                    types.Add(type.AsType(_metadataLoadContext));
                }

                foreach (var ns in current.GetNamespaceMembers())
                {
                    stack.Push(ns);
                }
            }
            return types.ToArray();
        }

        public override Type GetType(string name)
        {
            return Symbol.GetTypeByMetadataName(name).AsType(_metadataLoadContext);
        }
    }
}

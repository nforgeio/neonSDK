//-----------------------------------------------------------------------------
// FILE:        FromQueryAttributes.cs
// CONTRIBUTOR: Jeff Lill
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
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace Neon.ModelGen
{
    /// <summary>
    /// Used to indicate that a service endpoint parameter is to be obtained
    /// by parsing a request URI query parameter.
    /// </summary>
    /// <remarks>
    /// By default, this option will look for the query parameter with the same
    /// name as the tagged endpoint parameter.  This can be overriden by setting
    /// the <see cref="Name"/> property.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromQueryAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">
        /// Optionally specifies URI query parameter name.  This defaults to 
        /// the actual parameter name.
        /// </param>
        public FromQueryAttribute(string name = null)
        {
            this.Name = name;
        }

        /// <summary>
        /// Optionally overrides the tagged service endpoint method property
        /// name when generating the client code.
        /// </summary>
        public string Name { get; set; }
    }
}

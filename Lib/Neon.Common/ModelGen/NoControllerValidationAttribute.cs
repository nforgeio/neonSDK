//-----------------------------------------------------------------------------
// FILE:        NoControllerValidationAttribute.cs
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
using System.Text;

namespace Neon.ModelGen
{
    /// <summary>
    /// Used to prevent the <c>Neon.Xunit.XunitExtensions.ValidateController&lt;T&gt;()</c>
    /// method from including the tagged method when validating the service controller
    /// against its definining interface.  This is useful for rare situations where a
    /// service controller implements some extra endpoints that are not covered by the
    /// generated client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NoControllerValidationAttribute : Attribute
    {
    }
}

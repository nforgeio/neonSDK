// -----------------------------------------------------------------------------
// FILE:        Models/ElementRect.cs
// CONTRIBUTOR: NEONFORGE Team
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

using System.Text.Json.Serialization;

namespace Neon.Motion.Models
{
    /// <summary>
    /// A snapshot of an element's bounding rectangle returned by
    /// <see cref="JsInterop.GetRectAsync"/>.
    /// </summary>
    public record ElementRect(
        [property: JsonPropertyName("x")]      double X,
        [property: JsonPropertyName("y")]      double Y,
        [property: JsonPropertyName("width")]  double Width,
        [property: JsonPropertyName("height")] double Height);
}

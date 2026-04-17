// -----------------------------------------------------------------------------
// FILE:        Models/AnimationValues.cs
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

using System.Collections.Generic;
using System.Text.Json;

namespace Neon.Motion.Models
{
    /// <summary>
    /// Represents the target animation values passed to Motion's <c>animate()</c> function.
    /// Values can be single numbers, strings, or arrays of either for keyframe animations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use the fluent helpers for common CSS/transform properties, or index directly
    /// for any arbitrary property name:
    /// <code>
    /// var values = new AnimationValues()
    ///     .Opacity(0)
    ///     .X(100)
    ///     .Rotate(0, 360);   // keyframes
    ///
    /// // arbitrary property
    /// values["backgroundColor"] = "#ff0000";
    /// </code>
    /// </para>
    /// </remarks>
    public class AnimationValues : Dictionary<string, object>
    {
        // ------------------------------------------------------------------ //
        // Opacity / Visibility
        // ------------------------------------------------------------------ //

        public AnimationValues Opacity(double value)            { this["opacity"] = value;           return this; }
        public AnimationValues Opacity(params double[] frames)  { this["opacity"] = frames;          return this; }

        // ------------------------------------------------------------------ //
        // Independent transforms
        // ------------------------------------------------------------------ //

        public AnimationValues X(double value)                  { this["x"] = value;                 return this; }
        public AnimationValues X(params double[] frames)        { this["x"] = frames;                return this; }

        public AnimationValues Y(double value)                  { this["y"] = value;                 return this; }
        public AnimationValues Y(params double[] frames)        { this["y"] = frames;                return this; }

        public AnimationValues Z(double value)                  { this["z"] = value;                 return this; }
        public AnimationValues Z(params double[] frames)        { this["z"] = frames;                return this; }

        public AnimationValues Scale(double value)              { this["scale"] = value;             return this; }
        public AnimationValues Scale(params double[] frames)    { this["scale"] = frames;            return this; }

        public AnimationValues ScaleX(double value)             { this["scaleX"] = value;            return this; }
        public AnimationValues ScaleX(params double[] frames)   { this["scaleX"] = frames;           return this; }

        public AnimationValues ScaleY(double value)             { this["scaleY"] = value;            return this; }
        public AnimationValues ScaleY(params double[] frames)   { this["scaleY"] = frames;           return this; }

        public AnimationValues Rotate(double value)             { this["rotate"] = value;            return this; }
        public AnimationValues Rotate(params double[] frames)   { this["rotate"] = frames;           return this; }

        public AnimationValues RotateX(double value)            { this["rotateX"] = value;           return this; }
        public AnimationValues RotateX(params double[] frames)  { this["rotateX"] = frames;          return this; }

        public AnimationValues RotateY(double value)            { this["rotateY"] = value;           return this; }
        public AnimationValues RotateY(params double[] frames)  { this["rotateY"] = frames;          return this; }

        public AnimationValues RotateZ(double value)            { this["rotateZ"] = value;           return this; }
        public AnimationValues RotateZ(params double[] frames)  { this["rotateZ"] = frames;          return this; }

        public AnimationValues SkewX(double value)              { this["skewX"] = value;             return this; }
        public AnimationValues SkewX(params double[] frames)    { this["skewX"] = frames;            return this; }

        public AnimationValues SkewY(double value)              { this["skewY"] = value;             return this; }
        public AnimationValues SkewY(params double[] frames)    { this["skewY"] = frames;            return this; }

        // ------------------------------------------------------------------ //
        // Colors
        // ------------------------------------------------------------------ //

        public AnimationValues Color(string value)                  { this["color"] = value;             return this; }
        public AnimationValues Color(params string[] frames)        { this["color"] = frames;            return this; }

        public AnimationValues BackgroundColor(string value)        { this["backgroundColor"] = value;   return this; }
        public AnimationValues BackgroundColor(params string[] frames) { this["backgroundColor"] = frames; return this; }

        public AnimationValues BorderColor(string value)            { this["borderColor"] = value;       return this; }
        public AnimationValues BorderColor(params string[] frames)  { this["borderColor"] = frames;      return this; }

        public AnimationValues Fill(string value)                   { this["fill"] = value;              return this; }
        public AnimationValues Fill(params string[] frames)         { this["fill"] = frames;             return this; }

        public AnimationValues Stroke(string value)                 { this["stroke"] = value;            return this; }
        public AnimationValues Stroke(params string[] frames)       { this["stroke"] = frames;           return this; }

        // ------------------------------------------------------------------ //
        // Layout / Box model
        // ------------------------------------------------------------------ //

        public AnimationValues Width(string value)                  { this["width"] = value;             return this; }
        public AnimationValues Height(string value)                 { this["height"] = value;            return this; }

        public AnimationValues BorderRadius(string value)           { this["borderRadius"] = value;      return this; }
        public AnimationValues BorderRadius(params string[] frames) { this["borderRadius"] = frames;     return this; }

        // ------------------------------------------------------------------ //
        // Filters
        // ------------------------------------------------------------------ //

        public AnimationValues Blur(string value)                   { this["filter"] = $"blur({value})"; return this; }

        // ------------------------------------------------------------------ //
        // Generic setter (any CSS property or SVG attribute)
        // ------------------------------------------------------------------ //

        /// <summary>Sets any animation property by name. Value can be a number, string, double[], or string[].</summary>
        public AnimationValues Set(string property, object value)   { this[property] = value;            return this; }

        // ------------------------------------------------------------------ //
        // Serialization
        // ------------------------------------------------------------------ //

        internal string ToJson() => JsonSerializer.Serialize(this);
    }
}

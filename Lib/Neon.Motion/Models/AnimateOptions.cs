// -----------------------------------------------------------------------------
// FILE:        Models/AnimateOptions.cs
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neon.Motion.Models
{
    /// <summary>
    /// Options passed to Motion's <c>animate()</c> function.
    /// </summary>
    public class AnimateOptions
    {
        // ------------------------------------------------------------------ //
        // Animation type
        // ------------------------------------------------------------------ //

        /// <summary>
        /// The animation type. One of <c>"tween"</c>, <c>"spring"</c>, or <c>"inertia"</c>.
        /// Defaults to <c>"tween"</c>.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Type { get; set; }

        // ------------------------------------------------------------------ //
        // Tween options
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Duration of the animation in seconds. Default is 0.3 (0.8 for keyframes).
        /// </summary>
        [JsonPropertyName("duration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Duration { get; set; }

        /// <summary>
        /// Easing function. Can be a named easing string (e.g. <c>"easeInOut"</c>,
        /// <c>"circOut"</c>, <c>"linear"</c>) or a 4-element cubic-bezier array.
        /// </summary>
        [JsonPropertyName("ease")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Ease { get; set; }

        /// <summary>
        /// Keyframe timing positions as values from 0 to 1, one per keyframe.
        /// </summary>
        [JsonPropertyName("times")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double[] Times { get; set; }

        // ------------------------------------------------------------------ //
        // Spring options
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Spring bounciness, from 0 (no bounce) to 1 (very bouncy). Default is 0.25.
        /// Only used when <see cref="Type"/> is <c>"spring"</c>.
        /// </summary>
        [JsonPropertyName("bounce")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Bounce { get; set; }

        /// <summary>
        /// Spring visual duration override in seconds.
        /// </summary>
        [JsonPropertyName("visualDuration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? VisualDuration { get; set; }

        /// <summary>Spring stiffness. Default is 1.</summary>
        [JsonPropertyName("stiffness")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Stiffness { get; set; }

        /// <summary>Spring damping (opposing force). Default is 10.</summary>
        [JsonPropertyName("damping")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Damping { get; set; }

        /// <summary>Spring mass. Default is 1.</summary>
        [JsonPropertyName("mass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Mass { get; set; }

        /// <summary>Initial spring velocity.</summary>
        [JsonPropertyName("velocity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Velocity { get; set; }

        /// <summary>Speed threshold below which the spring is considered at rest. Default is 0.1.</summary>
        [JsonPropertyName("restSpeed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? RestSpeed { get; set; }

        /// <summary>Distance threshold below which the spring is considered at rest. Default is 0.01.</summary>
        [JsonPropertyName("restDelta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? RestDelta { get; set; }

        // ------------------------------------------------------------------ //
        // Timing
        // ------------------------------------------------------------------ //

        /// <summary>Delay in seconds before the animation starts. Default is 0.</summary>
        [JsonPropertyName("delay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Delay { get; set; }

        /// <summary>
        /// Number of times to repeat the animation. Set <see cref="RepeatInfinitely"/> to
        /// <c>true</c> to repeat indefinitely.
        /// </summary>
        [JsonPropertyName("repeat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Repeat { get; set; }

        /// <summary>
        /// When <c>true</c>, the animation repeats indefinitely (equivalent to
        /// <c>repeat: Infinity</c> in JavaScript).
        /// </summary>
        [JsonPropertyName("_repeatInfinite")]
        public bool RepeatInfinitely { get; set; }

        /// <summary>
        /// How to repeat: <c>"loop"</c>, <c>"reverse"</c>, or <c>"mirror"</c>. Default is <c>"loop"</c>.
        /// </summary>
        [JsonPropertyName("repeatType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string RepeatType { get; set; }

        /// <summary>Delay in seconds between repetitions. Default is 0.</summary>
        [JsonPropertyName("repeatDelay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? RepeatDelay { get; set; }

        // ------------------------------------------------------------------ //
        // Serialization
        // ------------------------------------------------------------------ //

        internal string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented           = false,
            });
    }
}

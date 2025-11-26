// -----------------------------------------------------------------------------
// FILE:	    UnitTest1.cs
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

using System;
using System.Collections.Generic;
using System.Drawing;

using FluentAssertions;

using Neon.ECharts.Options;
using Neon.ECharts.Options.Enum;
using Neon.ECharts.Options.Series;
using Neon.ECharts.Options.Series.Scatter;

namespace Test.Neon_ECharts
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var series = new Scatter3D()
            {
                Type = "scatter3D",
                Dimensions = new List<string>()
                {
                    "protein",
                    "fiber",
                    "sodium",
                    "fiber",
                    "vitaminc"
                },
                ItemStyle = new ItemStyle()
                {
                    BorderWidth = 1,
                    BorderColor = Color.FromArgb(200,Color.White)
                },
                SymbolSize = 12,
                Emphasis = new Emphasis()
                {
                    ItemStyle = new ItemStyle()
                    {
                        Color = ColorTranslator.FromHtml("#900")
                    }
                },
                Tooltip = new Tooltip()
                {
                    Trigger = TooltipTrigger.Item,
                    AxisPointer = new TooltipAxisPointer()
                    {
                        Type = AxisPointerType.Shadow
                    },
                    Formatter = new Neon.ECharts.Formatter()
                    {
                        JFunc = new JFunc("(params) => return params.value[6] + ' foo'")
                    }
                }
            };

            var s = EChartsOptionSerializer.Default.Serialize(series);

            Assert.NotNull(s);
        }

        [Fact]
        public void TestColorConverter()
        {
            var s = EChartsOptionSerializer.Default.Serialize(Color.White).Should().Be(@"""rgba(255, 255, 255, 1)""");

            var style = new AreaStyle()
            {
                Color       = Color.FromArgb(
                    alpha: Random.Shared.Next(255),
                    red:   Random.Shared.Next(255),
                    green: Random.Shared.Next(255),
                    blue:  Random.Shared.Next(255)),
            };

            var lineStyleString = EChartsOptionSerializer.Default.Serialize(style);

            var lineStyleDeserialized = EChartsOptionSerializer.Default.Deserialize<AreaStyle>(lineStyleString);

            lineStyleDeserialized.Color.Should().Be(style.Color);
            lineStyleDeserialized.ShadowColor.Should().Be(style.ShadowColor);
        }
    }
}
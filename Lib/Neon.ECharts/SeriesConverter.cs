// -----------------------------------------------------------------------------
// FILE:	    SeriesConverter.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;

using Neon.ECharts.Options.Series;
using Neon.ECharts.Options.Series.Bar;
using Neon.ECharts.Options.Series.Bar3D;
using Neon.ECharts.Options.Series.Boxplot;
using Neon.ECharts.Options.Series.Candlestick;
using Neon.ECharts.Options.Series.Custom;
using Neon.ECharts.Options.Series.EffectScatter;
using Neon.ECharts.Options.Series.FlowGL;
using Neon.ECharts.Options.Series.Funnel;
using Neon.ECharts.Options.Series.Gauge;
using Neon.ECharts.Options.Series.Globe;
using Neon.ECharts.Options.Series.Graph;
using Neon.ECharts.Options.Series.GraphGL;
using Neon.ECharts.Options.Series.Heatmap;
using Neon.ECharts.Options.Series.Line;
using Neon.ECharts.Options.Series.Line3D;
using Neon.ECharts.Options.Series.Lines;
using Neon.ECharts.Options.Series.Lines3D;
using Neon.ECharts.Options.Series.LinesGL;
using Neon.ECharts.Options.Series.Map;
using Neon.ECharts.Options.Series.Map3D;
using Neon.ECharts.Options.Series.Parallel;
using Neon.ECharts.Options.Series.PictorialBar;
using Neon.ECharts.Options.Series.Pie;
using Neon.ECharts.Options.Series.Radar;
using Neon.ECharts.Options.Series.Sankey;
using Neon.ECharts.Options.Series.Scatter;
using Neon.ECharts.Options.Series.Scatter3D;
using Neon.ECharts.Options.Series.ScatterGL;
using Neon.ECharts.Options.Series.Sunburst;
using Neon.ECharts.Options.Series.Surface;
using Neon.ECharts.Options.Series.ThemeRiver;
using Neon.ECharts.Options.Series.Tree;
using Neon.ECharts.Options.Series.Treemap;

namespace Neon.ECharts
{
    public class SeriesConverter : JsonConverter<ISeries>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(ISeries);
        }

        public override ISeries Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var readerClone = reader;
            var result      = default(ISeries);
            var type        = string.Empty;
            var depth       = 0;

            while (readerClone.Read()
                && string.IsNullOrEmpty(type))
            {
                JsonTokenType tokenType = readerClone.TokenType;

                switch (tokenType)
                {
                    case JsonTokenType.StartObject:

                        depth++;
                        continue;

                    case JsonTokenType.EndObject:

                        depth--;
                        continue;
                }

                if (tokenType == JsonTokenType.EndObject && depth == 0)
                {
                    return result;
                }

                // Get the key.

                if (tokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = readerClone.GetString();

                    if (propertyName == "type" && depth == 0)
                    {
                        type = JsonSerializer.Deserialize<string>(ref readerClone, options);
                    }
                }
            }

            result = type switch
            {
                Bar._Type           => JsonSerializer.Deserialize<Bar>(ref reader, options),
                Bar3D._Type         => JsonSerializer.Deserialize<Bar3D>(ref reader, options),
                Boxplot._Type       => JsonSerializer.Deserialize<Boxplot>(ref reader, options),
                Candlestick._Type   => JsonSerializer.Deserialize<Candlestick>(ref reader, options),
                Custom._Type        => JsonSerializer.Deserialize<Custom>(ref reader, options),
                EffectScatter._Type => JsonSerializer.Deserialize<EffectScatter>(ref reader, options),
                FlowGL._Type        => JsonSerializer.Deserialize<FlowGL>(ref reader, options),
                Funnel._Type        => JsonSerializer.Deserialize<Funnel>(ref reader, options),
                Gauge._Type         => JsonSerializer.Deserialize<Gauge>(ref reader, options),
                Globe._Type         => JsonSerializer.Deserialize<Globe>(ref reader, options),
                Graph._Type         => JsonSerializer.Deserialize<Graph>(ref reader, options),
                GraphGL._Type       => JsonSerializer.Deserialize<GraphGL>(ref reader, options),
                Heatmap._Type       => JsonSerializer.Deserialize<Heatmap>(ref reader, options),
                Line._Type          => JsonSerializer.Deserialize<Line>(ref reader, options),
                Line3D._Type        => JsonSerializer.Deserialize<Line3D>(ref reader, options),
                Lines._Type         => JsonSerializer.Deserialize<Lines>(ref reader, options),
                Lines3D._Type       => JsonSerializer.Deserialize<Lines3D>(ref reader, options),
                LinesGL._Type       => JsonSerializer.Deserialize<LinesGL>(ref reader, options),
                Map._Type           => JsonSerializer.Deserialize<Map>(ref reader, options),
                Map3D._Type         => JsonSerializer.Deserialize<Map3D>(ref reader, options),
                Parallel._Type      => JsonSerializer.Deserialize<Parallel>(ref reader, options),
                PictorialBar._Type  => JsonSerializer.Deserialize<PictorialBar>(ref reader, options),
                Pie._Type           => JsonSerializer.Deserialize<Pie>(ref reader, options),
                Radar._Type         => JsonSerializer.Deserialize<Radar>(ref reader, options),
                Sankey._Type        => JsonSerializer.Deserialize<Sankey>(ref reader, options),
                Scatter._Type       => JsonSerializer.Deserialize<Scatter>(ref reader, options),
                Scatter3D._Type     => JsonSerializer.Deserialize<Scatter3D>(ref reader, options),
                ScatterGL._Type     => JsonSerializer.Deserialize<ScatterGL>(ref reader, options),
                Sunburst._Type      => JsonSerializer.Deserialize<Sunburst>(ref reader, options),
                Surface._Type       => JsonSerializer.Deserialize<Surface>(ref reader, options),
                ThemeRiver._Type    => JsonSerializer.Deserialize<ThemeRiver>(ref reader, options),
                Tree._Type          => JsonSerializer.Deserialize<Tree>(ref reader, options),
                Treemap._Type       => JsonSerializer.Deserialize<Treemap>(ref reader, options),
                _ => throw new NotImplementedException(),
            };

            return result;

        }

        public override void Write(Utf8JsonWriter writer, ISeries value, JsonSerializerOptions options)
        {
            var type = value.Type;

             switch (type)
             {
                case Bar._Type:
                    JsonSerializer.Serialize(writer, (Bar)value, options);
                    break;

                case Bar3D._Type:
                    JsonSerializer.Serialize(writer, (Bar3D)value, options);
                    break;

                case Boxplot._Type:
                    JsonSerializer.Serialize(writer, (Boxplot)value, options);
                    break;

                case Candlestick._Type:
                    JsonSerializer.Serialize(writer, (Candlestick)value, options);
                    break;

                case Custom._Type:
                    JsonSerializer.Serialize(writer, (Custom)value, options);
                    break;

                case EffectScatter._Type:
                    JsonSerializer.Serialize(writer, (EffectScatter)value, options);
                    break;

                case FlowGL._Type:
                    JsonSerializer.Serialize(writer, (FlowGL)value, options);
                    break;

                case Funnel._Type:
                    JsonSerializer.Serialize(writer, (Funnel)value, options);
                    break;

                case Gauge._Type:
                    JsonSerializer.Serialize(writer, (Gauge)value, options);
                    break;

                case Globe._Type:
                    JsonSerializer.Serialize(writer, (Globe)value, options);
                    break;

                case Graph._Type:
                    JsonSerializer.Serialize(writer, (Graph)value, options);
                    break;

                case GraphGL._Type:
                    JsonSerializer.Serialize(writer, (GraphGL)value, options);
                    break;

                case Heatmap._Type:
                    JsonSerializer.Serialize(writer, (Heatmap)value, options);
                    break;

                case Line._Type:
                    JsonSerializer.Serialize(writer, (Line)value, options);
                    break;

                case Line3D._Type:
                    JsonSerializer.Serialize(writer, (Line3D)value, options);
                    break;

                case Lines._Type:
                    JsonSerializer.Serialize(writer, (Lines)value, options);
                    break;

                case Lines3D._Type:
                    JsonSerializer.Serialize(writer, (Lines3D)value, options);
                    break;

                case LinesGL._Type:
                    JsonSerializer.Serialize(writer, (LinesGL)value, options);
                    break;

                case Map._Type:
                    JsonSerializer.Serialize(writer, (Map)value, options);
                    break;

                case Map3D._Type:
                    JsonSerializer.Serialize(writer, (Map3D)value, options);
                    break;

                case Parallel._Type:
                    JsonSerializer.Serialize(writer, (Parallel)value, options);
                    break;

                case PictorialBar._Type:
                    JsonSerializer.Serialize(writer, (PictorialBar)value, options);
                    break;

                case Pie._Type:
                    JsonSerializer.Serialize(writer, (Pie)value, options);
                    break;

                case Radar._Type:
                    JsonSerializer.Serialize(writer, (Radar)value, options);
                    break;

                case Sankey._Type:
                    JsonSerializer.Serialize(writer, (Sankey)value, options);
                    break;

                case Scatter._Type:
                    JsonSerializer.Serialize(writer, (Scatter)value, options);
                    break;

                case Scatter3D._Type:
                    JsonSerializer.Serialize(writer, (Scatter3D)value, options);
                    break;

                case ScatterGL._Type:
                    JsonSerializer.Serialize(writer, (ScatterGL)value, options);
                    break;

                case Sunburst._Type:
                    JsonSerializer.Serialize(writer, (Sunburst)value, options);
                    break;

                case Surface._Type:
                    JsonSerializer.Serialize(writer, (Surface)value, options);
                    break;

                case ThemeRiver._Type:
                    JsonSerializer.Serialize(writer, (ThemeRiver)value, options);
                    break;

                case Tree._Type:
                    JsonSerializer.Serialize(writer, (Tree)value, options);
                    break;

                case Treemap._Type:
                    JsonSerializer.Serialize(writer, (Treemap)value, options);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ;
        }
    }
}

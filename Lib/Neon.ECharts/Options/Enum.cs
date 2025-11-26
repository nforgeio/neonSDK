// -----------------------------------------------------------------------------
// FILE:	    Enum.cs
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

namespace Neon.ECharts.Options.Enum
{
    public enum Align
    {
        Auto,
        Left,
        Right,
        Center
    }


    /// <summary>
    /// Specifies the alignment options.
    /// </summary>
    public enum Align1
    {
        /// <summary>
        /// Automatically determines the alignment.
        /// </summary>
        Auto,

        /// <summary>
        /// Aligns to the left.
        /// </summary>
        Left,

        /// <summary>
        /// Aligns to the right.
        /// </summary>
        Right
    }


    public enum Align2
    {
        Left,
        Right,
        Center
    }


    public enum VerticalAlign
    {
        Top,
        Middle,
        Bottom
    }


    public enum AxisType
    {
        Value,
        Category,
        Time,
        Log
    }


    public enum Location
    {
        Start,
        Middle,
        End
    }


    public enum PositionY
    {
        Left,
        Right
    }


    public enum PositionX
    {
        Top,
        Bottom
    }


    public enum AxisPointerAxis
    {
        X,
        Y,
        Radius,
        Angle
    }


    public enum AxisPointerType
    {
        Line,
        Shadow,
        None,
        Cross
    }


    public enum TooltipTrigger
    {
        Item,
        Axis,
        None
    }


    public enum CoordinateSystem
    {
        Cartesian2d,
        Polar,
        Bmap,
        Geo
    }


    public enum Origin
    {
        Auto,
        Start,
        End
    }


    public enum FillColorRepeat
    {
        RepeatX,
        RepeatY,
        NoRepeat
    }


    public enum Target
    {
        Self,
        Blank
    }


    public enum FontStyle
    {
        Normal,
        Italic,
        Oblique
    }


    public enum FontWeight
    {
        Normal,
        Bold,
        Bolder,
        Lighter
    }


    public enum LegendType
    {
        plain,
        scroll
    }


    public enum Orient
    {
        Horizontal,
        Vertical
    }


    public enum SelectorPosition
    {
        Start,
        End
    }


    public enum FilterMode
    {
        Filter,
        WeakFilter,
        Empty,
        None
    }


    public enum RangeMode
    {
        Value,
        Percent
    }


    public enum LineStyleType
    {
        Solid,
        Dashed,
        Dotted
    }


    public enum Sampling
    {
        Average,
        Max,
        Min,
        Sum
    }


    public enum ColorType
    {
        Linear,
        Radial
    }


    public enum MagicTypeType
    {
        Line,
        Bar,
        Stack,
        Tiled
    }

    public enum MarkPointDataType
    {
        Average,
        Max,
        Min
    }


    public enum LabelPosition
    {
        Uutside,
        Inside,
        Inner,
        Center,
        Top,
        Right
    }


    public enum SelectedMode
    {
        Single,
        Multiple
    }


    public enum RadarShape
    {
        Polygon,
        Circle
    }

    public enum BrushType
    {
        Rect,
        Polygon,
        LineX,
        LineY,
        Keep,
        Clear
    }
    public enum SortType
    {
        Ascending,
        Descending,
        None
    }
    public enum Layout
    {
        None,
        Circular,
        Force
    }
    public enum EventType
    {
        click,

        dblclick,

        mousedown,

        mousemove,

        mouseup,

        mouseover,

        mouseout,

        globalout,

        contextmenu,

        legendselectchanged,

        legendunselected,

        legendselectall,

        legendinverseselect,

        legendscroll,

        datazoom,

        datarangeselected,

        timelinechanged,

        timelineplaychanged,

        restore,

        dataviewchanged,

        magictypechanged,

        geoselectchanged,

        geoselected,

        geounselected,

        pieselectchanged,

        pieselected,

        pieunselected,

        mapselectchanged,

        mapselected,

        mapunselected,

        axisareaselected,

        focusnodeadjacency,

        unfocusnodeadjacency,

        brush,

        brushEnd,

        brushselected,

        globalcursortaken,

        rendered,

        finished
    }

    public enum MoveOverlap
    {
        ShiftX,
        ShiftY
    }
}

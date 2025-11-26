// -----------------------------------------------------------------------------
// FILE:	    Events.cs
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

namespace Neon.Mapbox
{
    public enum Events
    {
        /// <summary>
        /// Fired when the user cancels a "box zoom" interaction, or when the bounding box does not meet the minimum size threshold. 
        /// </summary>
        boxzoomcancel,

        /// <summary>
        /// Fired when a "box zoom" interaction ends.
        /// </summary>
        boxzoomend,

        /// <summary>
        /// Fired when a "box zoom" interaction starts. 
        /// </summary>
        boxzoomstart,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) is pressed and released at the same point on the map. 
        /// • This event is compatible with the optional layerId parameter. If layerId is included as the second argument in Map#on, 
        /// the event listener will fire only when the point that is pressed and released contains a visible portion of the specifed layer.
        /// </summary>
        click,

        /// <summary>
        /// Fired when the right button of the mouse is clicked or the context menu key is pressed within the map.
        /// </summary>
        contextmenu,

        /// <summary>
        /// Fired when any map data loads or changes.
        /// </summary>
        data,

        /// <summary>
        /// Fired when any map data (style, source, tile, etc) begins loading or changing asyncronously. All dataloading events are followed by a data or error event.
        /// </summary>
        dataloading,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) is pressed and released twice at the same point on the map in rapid succession.
        /// </summary>
        dblclick,

        /// <summary>
        /// Fired repeatedly during a "drag to pan" interaction.
        /// </summary>
        drag,

        /// <summary>
        /// Fired when a "drag to pan" interaction ends.
        /// </summary>
        dragend,

        /// <summary>
        /// Fired when a "drag to pan" interaction starts.
        /// </summary>
        dragstart,

        /// <summary>
        /// Fired when an error occurs. This is GL JS's primary error reporting mechanism. We use an event instead of throw to better accommodate asyncronous operations. 
        /// If no listeners are bound to the error event, the error will be printed to the console.
        /// </summary>
        error,

        /// <summary>
        /// Fired after the last frame rendered before the map enters an "idle" state: 
        /// • No camera transitions are in progress 
        /// • All currently requested tiles have loaded 
        /// • All fade/transition animations have completed
        /// </summary>
        idle,

        /// <summary>
        /// Fired immediately after all necessary resources have been downloaded and the first visually complete rendering of the map has occurred.
        /// </summary>
        load,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) is pressed within the map.
        /// • This event is compatible with the optional layerId parameter. If layerId is included as the second argument in Map#on, 
        /// the event listener will fire only when the the cursor is pressed while inside a visible portion of the specifed layer.
        /// </summary>
        mousedown,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) enters a visible portion of a specified layer from outside that layer or outside the map canvas.
        /// • Important: This event can only be listened for when Map#on includes three arguments, where the second argument specifies the desired layer.
        /// </summary>
        mouseenter,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) leaves a visible portion of a specified layer, or leaves the map canvas.
        /// • Important: This event can only be listened for when Map#on includes three arguements, where the second argument specifies the desired layer.
        /// </summary>
        mouseleave,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) is moved while the cursor is inside the map. As you move the cursor across the map, 
        /// the event will fire every time the cursor changes position within the map.
        /// • Note: This event is compatible with the optional layerId parameter. If layerId is included as the second argument in Map#on, 
        /// the event listener will fire only when the the cursor is inside a visible portion of the specified layer.
        /// </summary>
        mousemove,

        /// <summary>
        /// Fired when a point device (usually a mouse) leaves the map's canvas.
        /// </summary>
        mouseout,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) is moved within the map. As you move the cursor across a web page containing a map, the event will fire each time it enters the map or any child elements.
        /// • Note: This event is compatible with the optional layerId parameter. If layerId is included as the second argument in Map#on, 
        /// the event listener will fire only when the the cursor is moved inside a visible portion of the specifed layer.
        /// </summary>
        mouseover,

        /// <summary>
        /// Fired when a pointing device (usually a mouse) is released within the map.
        /// • Note: This event is compatible with the optional layerId parameter. If layerId is included as the second argument in Map#on, 
        /// the event listener will fire only when the the cursor is released while inside a visible portion of the specifed layer.
        /// </summary>
        mouseup,

        /// <summary>
        /// Fired repeatedly during an animated transition from one view to another, as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        move,

        /// <summary>
        /// Fired just after the map completes a transition from one view to another, as the result of either user interaction or methods such as Map#jumpTo.
        /// </summary>
        moveend,

        /// <summary>
        /// Fired just before the map begins a transition from one view to another, as the result of either user interaction or methods such as Map#jumpTo.
        /// </summary>
        movestart,

        /// <summary>
        /// Fired repeatedly during the map's pitch (tilt) animation between one state and another as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        pitch,

        /// <summary>
        /// Fired immediately after the map's pitch (tilt) finishes changing as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        pitchend,

        /// <summary>
        /// Fired whenever the map's pitch (tilt) begins a change as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        pitchstart,

        /// <summary>
        /// Fired immediately after the map has been removed with Map.event:remove.
        /// </summary>
        remove,

        /// <summary>
        /// Fired whenever the map is drawn to the screen, as the result of
        /// • a change to the map's position, zoom, pitch, or bearing
        /// • a change to the map's style
        /// • a change to a GeoJSON source
        /// • the loading of a vector tile, GeoJSON file, glyph, or sprite
        /// </summary>
        render,

        /// <summary>
        /// Fired immediately after the map has been resized.
        /// </summary>
        resize,

        /// <summary>
        /// Fired repeatedly during a "drag to rotate" interaction
        /// </summary>
        rotate,

        /// <summary>
        /// Fired when a "drag to rotate" interaction ends. 
        /// </summary>
        rotateend,

        /// <summary>
        /// Fired when a "drag to rotate" interaction starts
        /// </summary>
        rotatestart,

        /// <summary>
        /// Fired when one of the map's sources loads or changes, including if a tile belonging to a source loads or changes. 
        /// </summary>
        sourcedata,

        /// <summary>
        /// Fired when one of the map's sources begins loading or changing asyncronously. All sourcedataloading events are followed by a sourcedata or error event. 
        /// </summary>
        sourcedataloading,

        /// <summary>
        /// Fired when the map's style loads or changes.
        /// </summary>
        styledata,

        /// <summary>
        /// Fired when the map's style begins loading or changing asyncronously. All styledataloading events are followed by a styledata or error event. 
        /// </summary>
        styledataloading,

        /// <summary>
        /// Fired when an icon or pattern needed by the style is missing. The missing image can be added with Map#addImage within this event listener 
        /// callback to prevent the image from being skipped. This event can be used to dynamically generate icons and patterns.
        /// </summary>
        styleimagemissing,

        /// <summary>
        /// Fired when a touchcancel event occurs within the map.
        /// </summary>
        touchcancel,

        /// <summary>
        /// Fired when a touchend event occurs within the map.
        /// </summary>
        touchend,

        /// <summary>
        /// Fired when a touchmove event occurs within the map.
        /// </summary>
        touchmove,

        /// <summary>
        /// Fired when a touchstart event occurs within the map.
        /// </summary>
        touchstart,

        /// <summary>
        /// Fired when the WebGL context is lost.
        /// </summary>
        webglcontextlost,

        /// <summary>
        /// Fired when the WebGL context is restored.
        /// </summary>
        webglcontextrestored,

        /// <summary>
        /// Fired when a wheel event occurs within the map.
        /// </summary>
        wheel,

        /// <summary>
        /// Fired repeatedly during an animated transition from one zoom level to another, as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        zoom,

        /// <summary>
        /// Fired just after the map completes a transition from one zoom level to another, as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        zoomend,

        /// <summary>
        /// Fired just before the map begins a transition from one zoom level to another, as the result of either user interaction or methods such as Map#flyTo.
        /// </summary>
        zoomstart
    }
}

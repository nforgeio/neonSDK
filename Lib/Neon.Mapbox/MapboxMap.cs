// -----------------------------------------------------------------------------
// FILE:	    MapboxMap.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

using Neon.Mapbox.Models;
using Neon.Tasks;

using NetTopologySuite.IO.Converters;

namespace Neon.Mapbox
{
    public partial class MapboxMap : ComponentBase, IAsyncDisposable
    {
        public bool IsLoaded { get; private set; } = false;

        private static JsonSerializerOptions jsonOptions;
        public static JsonSerializerOptions DefaultSerializerOptions
        {
            get
            {
                if (jsonOptions == null)
                {
                    jsonOptions = new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                    };

                    jsonOptions.Converters.Add(new GeoJsonConverterFactory());
                }

                return jsonOptions;
            }
            set
            {
                jsonOptions = value;
            }
        }
        private string accessToken { get; set; }
        internal static string globalAccessToken;

        /// <summary>
        /// The reference to our exported module. 
        /// </summary>
        public IJSObjectReference Module;

        private readonly ConcurrentDictionary<Guid, DotNetObjectReference<CallbackAction>> References = new ConcurrentDictionary<Guid, DotNetObjectReference<CallbackAction>>();

        private DotNetObjectReference<MapboxMap> DotNetObjectReference;

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private GeoJsonConverterFactory GeoJsonConverterFactory { get; set; }

        /// <summary>
        /// The identifier for the HTML element.
        /// </summary>
        public string Id { get; private set; } = "mapbox_" + Guid.NewGuid();

        [Parameter]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "<Pending>")]
        public string AccessToken
        {
            get
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    return accessToken;
                }

                return globalAccessToken;
            }
            set => accessToken = value;
        }

        [Parameter]
        public string Height { get; set; } = "500px";

        [Parameter]
        public string Width { get; set; } = "100%";

        [Parameter]
        public Options Options { get; set; }

        [Parameter]
        public EventCallback<EventArgs> OnLoad { get; set; }

        [JSInvokable]
        public async Task OnLoadCallback()
        {
            IsLoaded = true;
            await OnLoad.InvokeAsync(EventArgs.Empty);
        }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "id", Id);

            if (AdditionalAttributes != null)
            {
                var sequence = 2;
                foreach (var attribute in AdditionalAttributes)
                {
                    builder.AddAttribute(sequence, attribute.Key, attribute.Value);
                    sequence++;
                }
            }

            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                builder.AddContent(3, new MarkupString("<h3>Could not load map. Mapbox requires an access token to use the API.</h3> <a href='https://docs.mapbox.com/help/how-mapbox-works/access-tokens/'>https://docs.mapbox.com/help/how-mapbox-works/access-tokens/</a>"));
            }

            builder.CloseElement();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SyncContext.Clear;

            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "https://api.mapbox.com/mapbox-gl-js/v2.0.0/mapbox-gl.js");

                Module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");
                await Module.InvokeVoidAsync("AddStylesheet");

                DotNetObjectReference = Microsoft.JSInterop.DotNetObjectReference.Create(this);

                await InvokeAsync(StateHasChanged);

                await Create();
            }
        }

        private ValueTask Create()
        {
            // Set the container identifier.
            if (Options == null)
            {
                Options = new Options();
            }
            Options.Container = Id;

            return Module.InvokeVoidAsync("Mapbox.create", AccessToken, Options, DotNetObjectReference);
        }

        public async Task<Listener> AddListener<T>(Events eventName, string layer, Action<T> handler)
        {
            var callback = new CallbackAction(Module, eventName.ToString(), handler, typeof(T));
            var reference = Microsoft.JSInterop.DotNetObjectReference.Create(callback);
            References.TryAdd(Guid.NewGuid(), reference);

            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            if (string.IsNullOrWhiteSpace(layer))
            {
                await Module.InvokeVoidAsync("Mapbox.on", Id, eventName.ToString(), reference);
            }
            else
            {
                await Module.InvokeVoidAsync("Mapbox.on", Id, eventName.ToString(), reference, layer);
            }            

            return new Listener(callback);
        }

        public async Task<Popup> GetPopup(PopupOptions options)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            var popup = new Popup(Module, Id);
            await Module.InvokeVoidAsync($"MapboxPopup.create", popup.ContainerId, options);

            return popup;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var value in References.Values)
            {
                value?.Dispose();
            }

            DotNetObjectReference?.Dispose();

            if (Module != null)
            {
                await Module.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        #region Instance Members

        /// <summary>
        /// Adds a Mapbox style layer to the map's style.
        /// </summary>
        /// <param name="layer">The layer to add, conforming to either the Mapbox Style Specification's layer definition or, less commonly, the CustomLayerInterface specification. </param>
        /// <param name="beforeId">The ID of an existing layer to insert the new layer before, resulting in the new layer appearing visually beneath the existing layer. 
        /// If this argument is not specified, the layer will be appended to the end of the layers array and appear visually above all other layers.</param>
        public async ValueTask AddLayer(Layer layer, string beforeId = null)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.addLayer", Id, layer, beforeId);
        }

        /// <summary>
        /// Removes the layer with the given ID from the map's style.
        /// </summary>
        /// <param name="id">The ID of the layer to remove.</param>
        public async ValueTask RemoveLayer(string id)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.removeLayer", Id, id);
        }

        /// <summary>
        /// Adds a source to the map's style.
        /// </summary>
        /// <param name="id">The ID of the source to add. Must not conflict with existing sources.</param>
        /// <param name="source">The source object, conforming to the Mapbox Style Specification's source definition or CanvasSourceOptions.</param>
        public async ValueTask AddSource<T>(string id, T source)
            where T : Source
        {
            source.Map = this;

            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.addSource", Id, id, JsonSerializer.Serialize(source, DefaultSerializerOptions));
        }

        /// <summary>
        /// Removes a source from the map's style.
        /// </summary>
        /// <param name="id">The ID of the layer to remove.</param>
        public async ValueTask RemoveSource(string id)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.removeSource", Id, id);
        }

        /// <summary>
        /// Pans and zooms the map to contain its visible area within the specified geographical bounds. This function will also reset the map's bearing to 0 if bearing is nonzero.
        /// </summary>
        /// <param name="bounds">Center these bounds in the viewport and use the highest zoom level up to and including Map#getMaxZoom() that fits them in the viewport.</param>
        public async ValueTask FitBounds(LngLatBounds bounds)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync($"Mapbox.fitBounds", Id, bounds);
        }

        /// <summary>
        /// Returns the map's geographical centerpoint.
        /// </summary>
        /// <returns><see cref="LatLng">LngLat</see>: The map's geographical centerpoint.</returns>
        public async ValueTask<LngLat> GetCenter()
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            return await Module.InvokeAsync<LngLat>($"Mapbox.getCenter", Id);
        }

        /// <summary>
        /// Returns a Point representing pixel coordinates, relative to the map's container, that correspond to the specified geographical location.
        /// When the map is pitched and lnglat is completely behind the camera, there are no pixel coordinates corresponding to that location.
        /// In that case, the x and y components of the returned Point are set to Number.MAX_VALUE.
        /// </summary>
        /// <param name="coordinate">The geographical location to project.</param>
        /// <returns>The Point corresponding to lnglat, relative to the map's container.</returns>
        public async ValueTask<Point> Project(LngLat coordinate)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            return await Module.InvokeAsync<Point>("Mapbox.project", Id, coordinate);
        }

        /// <summary>
        /// Resizes the map according to the dimensions of its container element.  Checks if the map container size changed and updates the map if it has changed.
        /// This method must be called after the map's container is resized programmatically or when the map is shown after being initially hidden with CSS.
        /// </summary>
        public async ValueTask Resize()
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync($"Mapbox.resize", Id);
        }

        /// <summary>
        /// Sets the state of a feature. A feature's state is a set of user-defined key-value pairs that are assigned to a feature at runtime. 
        /// When using this method, the state object is merged with any existing key-value pairs in the feature's state. 
        /// Features are identified by their feature.id attribute, which can be any number or string.       
        /// This method can only be used with sources that have a feature.id attribute.The feature.id attribute can be defined in three ways:
        /// • For vector or GeoJSON sources, including an id attribute in the original data file.
        /// • For vector or GeoJSON sources, using the promoteId option at the time the source is defined.
        /// • For GeoJSON sources, using the generateId option to auto-assign an id based on the feature's index in the source data. 
        /// If you change feature data using map.getSource('some id').setData(..), you may need to re-apply state taking into account updated id values.
        /// Note: You can use the feature-state expression to access the values in a feature's state object for the purposes of styling.
        /// </summary>
        /// <param name="feature">Feature identifier. </param>
        /// <param name="state">A set of key-value pairs. The values should be valid JSON types.</param>
        public async ValueTask SetFeatureState(Feature feature, Dictionary<string, object> state)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.setFeatureState", Id, feature, state);
        }

        /// <summary>
        /// <para>
        /// Changes any combination of center, zoom, bearing, and pitch, animating the transition along a curve that evokes
        /// flight. The animation seamlessly incorporates zooming and panning to help the user maintain their bearings even
        /// after traversing a great distance.
        /// </para>
        /// <note>
        /// The transition will happen instantly if the user has enabled the reduced motion accessibility feature enabled in their operating system, unless
        /// options includes essential: true.
        /// </note>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async ValueTask FlyTo(CameraOptions options, object eventData = null)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.flyTo", Id, options, eventData);
        }

        public async ValueTask<T> GetSource<T>(string id)
            where T : Source
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            var source       = await Module.InvokeAsync<dynamic>("Mapbox.getSource", Id, id);
            var sourceString = JsonSerializer.Serialize(source, DefaultSerializerOptions);
            var result       = JsonSerializer.Deserialize<T>(sourceString, DefaultSerializerOptions);

            result.Id  = id;
            result.Map = this;

            return result;
        }

        public async ValueTask SetSourceData<T>(string id, T data)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.setSourceData", Id, id, JsonSerializer.Serialize(data, DefaultSerializerOptions));
        }

        public async ValueTask UpdateSourceData<T>(string id, T data)
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync("Mapbox.updateSourceData", Id, id, JsonSerializer.Serialize(data, DefaultSerializerOptions));
        }

        public async ValueTask ResizeContainer()
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync($"ResizeMapContainer", Id);
            await Resize();

            await InvokeAsync(StateHasChanged);
        }

        public async ValueTask RemoveStyle()
        {
            Module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Mapbox/MapboxInterop.js");

            await Module.InvokeVoidAsync($"RemoveMapContainerStyle", Id);
        }

        #endregion 

    }
}

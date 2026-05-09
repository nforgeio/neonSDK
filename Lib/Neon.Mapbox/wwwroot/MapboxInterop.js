function AddStylesheet() {
    var link = document.createElement('link');
    link.rel = "stylesheet";
    link.href = "https://api.mapbox.com/mapbox-gl-js/v2.0.0/mapbox-gl.css";
    document.head.appendChild(link)
}

function waitForElm(selector) {
    return new Promise(resolve => {
        if (document.querySelector(selector)) {
            return resolve(document.querySelector(selector));
        }

        const observer = new MutationObserver(mutations => {
            if (document.querySelector(selector)) {
                observer.disconnect();
                resolve(document.querySelector(selector));
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    });
}
function RemoveMapContainerStyle(container) {
    let divElement = document.getElementById(container);
    divElement.style = null;
}

function ResizeMapContainer(container) {
    console.log("Resizing Container: " + container);

    var divElement = document.getElementById(container);
    var height = document.defaultView.getComputedStyle(divElement.parentNode).height;
    var width = document.defaultView.getComputedStyle(divElement.parentNode).width;

    console.log("setting container " + container + " height to " + height + " and width to " + width);

    divElement.style.height = height;
    divElement.style.width = width;
}

window.mapboxInstances = {};

var Mapbox = {
    create: async function (accessToken, options, dotnetReference) {
        mapboxgl.accessToken = accessToken;

        await waitForElm('#' + options.container);

        var map = new mapboxgl.Map(options);
        window.mapboxInstances[options.container] = map;
        map.on('load', function () {
            dotnetReference.invokeMethodAsync("OnLoadCallback")
        });
    },
    addLayer: function (container, layer, beforeId) {
        window.mapboxInstances[container].addLayer(layer, beforeId);
    },
    removeLayer: function (container, id) {
        window.mapboxInstances[container].removeLayer(id);
    },
    addSource: function (container, id, source) {
        var parsedSource = JSON.parse(source);
        window.mapboxInstances[container].addSource(id, parsedSource);
    },
    addImage: function (container, id, source) {
        var parsedSource = JSON.parse(source);
        window.mapboxInstances[container].addImage(id, parsedSource);
    },
    removeSource: function (container, id) {
        window.mapboxInstances[container].removeSource(id);
    },
    getSource: function (container, id) {
        var result = window.mapboxInstances[container].getSource(id).serialize();
        console.log(result);
        return result;
    },
    setSourceData: function (container, id, data) {
        console.log("Container: " + container);
        console.log("Id: " + id);
        console.log("Data: " + data);

        var parsedSource = JSON.parse(data);
        var source = window.mapboxInstances[container].getSource(id);

        source.setData(parsedSource);
    },
    fitBounds: function (container, bounds) {
        var llb = new mapboxgl.LngLatBounds(bounds.sw, bounds.ne);
        window.mapboxInstances[container].fitBounds(llb);
    },
    getCenter: function (container) {
        return window.mapboxInstances[container].getCenter();
    },
    project: function (container, coordinate) {
        return window.mapboxInstances[container].project(coordinate);
    },
    resize: function (container) {
        window.mapboxInstances[container].resize();
    },
    easeTo: function (container, options, eventData) {
        window.mapboxInstances[container].easeTo(options, eventData);
    },
    flyTo: function (container, options, eventData) {
        window.mapboxInstances[container].flyTo(options, eventData);
    },
    setFeatureState: function (container, feature, state) {
        window.mapboxInstances[container].setFeatureState(feature, state);
    },
    on: (container, eventType, dotnetReference, args) => {
        if (args === undefined) {
            window.mapboxInstances[container].on(eventType, function (e) {
                e.target = null; // Remove map to prevent circular references.
                const result = JSON.stringify(e);
                dotnetReference.invokeMethodAsync('Invoke', result)
            })
        }
        else {
            window.mapboxInstances[container].on(eventType, args, function (e) {
                e.target = null; // Remove map to prevent circular references.
                const result = JSON.stringify(e);
                dotnetReference.invokeMethodAsync('Invoke', result)
            })
        }
    }
}

const popups = {};

var MapboxPopup = {
    create: function (popupId, options) {
        var popup = new mapboxgl.Popup(options);
        popups[popupId] = popup;
    },
    addClassName: function (popupId, className) {
        popups[popupId].addClassName(className);
    },
    addTo: function (popupId, mapId) {
        var map = window.mapboxInstances[mapId];
        popups[popupId].addTo(map);
    },
    getLngLat: function (popupId) {
        return popups[popupId].getLngLat();
    },
    getMaxWidth: function (popupId) {
        return popups[popupId].getMaxWidth();
    },
    isOpen: function (popupId) {
        return popups[popupId].isOpen();
    },
    remove: function (popupId) {
        popups[popupId].remove();
    },
    removeClassName: function (popupId, className) {
        popups[popupId].removeClassName(className);
    },
    setLngLat: function (popupId, lnglat) {
        popups[popupId].setLngLat(lnglat);
    },
    setText: function (popupId, text) {
        popups[popupId].setText(text);
    },
    toggleClassName: function (popupId, className) {
        return popups[popupId].toggleClassName(className);
    },
    on: (popupId, eventType, dotnetReference) => {
        popups[popupId].on(eventType, function () {
            dotnetReference.invokeMethodAsync('InvokeWithoutArgs')
        })
    }
}

export { Mapbox, MapboxPopup, AddStylesheet, RemoveMapContainerStyle, ResizeMapContainer };
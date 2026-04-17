// motion-interop.js — Blazor JS interop bridge for motion.dev
import { animate, scroll, inView, stagger } from "https://cdn.jsdelivr.net/npm/motion@latest/+esm";

// Active animation controls keyed by animId
const animations = new Map();

// Cancellation functions for scroll() keyed by scrollId
const scrollCancels = new Map();

// Stop functions for inView() keyed by inViewId
const inViewStops = new Map();

// ---------------------------------------------------------------------------
// animate()
// ---------------------------------------------------------------------------

export function motionAnimate(animId, selector, valuesJson, optionsJson) {
    _cancelAnimation(animId);

    const values  = JSON.parse(valuesJson);
    const options = _parseOptions(optionsJson);

    const anim = animate(selector, values, options);
    animations.set(animId, anim);
}

export function motionAnimateWithCallbacks(animId, selector, valuesJson, optionsJson, dotnetRef) {
    _cancelAnimation(animId);

    const values  = JSON.parse(valuesJson);
    const options = _parseOptions(optionsJson);

    const originalOnComplete = options.onComplete;
    options.onComplete = () => {
        dotnetRef.invokeMethodAsync("OnComplete", animId);
        if (originalOnComplete) originalOnComplete();
    };

    const anim = animate(selector, values, options);
    animations.set(animId, anim);
}

// Animate with stagger delay across multiple elements
export function motionAnimateStagger(animId, selector, valuesJson, optionsJson, staggerSeconds) {
    _cancelAnimation(animId);

    const values  = JSON.parse(valuesJson);
    const options = _parseOptions(optionsJson);
    options.delay = stagger(staggerSeconds);

    const anim = animate(selector, values, options);
    animations.set(animId, anim);
}

export function motionPause(animId) {
    animations.get(animId)?.pause();
}

export function motionPlay(animId) {
    animations.get(animId)?.play();
}

export function motionCancel(animId) {
    _cancelAnimation(animId);
}

export function motionComplete(animId) {
    animations.get(animId)?.complete();
}

export function motionStop(animId) {
    animations.get(animId)?.stop();
    animations.delete(animId);
}

export function motionGetTime(animId) {
    return animations.get(animId)?.time ?? 0;
}

export function motionSetTime(animId, time) {
    const anim = animations.get(animId);
    if (anim) anim.time = time;
}

export function motionGetSpeed(animId) {
    return animations.get(animId)?.speed ?? 1;
}

export function motionSetSpeed(animId, speed) {
    const anim = animations.get(animId);
    if (anim) anim.speed = speed;
}

// ---------------------------------------------------------------------------
// scroll()
// ---------------------------------------------------------------------------

export function motionScroll(scrollId, targetSelector, containerSelector, optionsJson, dotnetRef) {
    _cancelScroll(scrollId);

    const options = _parseOptions(optionsJson);

    if (targetSelector) {
        options.target = document.querySelector(targetSelector);
    }
    if (containerSelector) {
        options.container = document.querySelector(containerSelector);
    }

    const cancel = scroll((progress, info) => {
        const infoJson = info
            ? JSON.stringify({ x: info.x, y: info.y })
            : null;
        dotnetRef.invokeMethodAsync("OnScroll", scrollId, progress, infoJson);
    }, options);

    scrollCancels.set(scrollId, cancel);
}

export function motionCancelScroll(scrollId) {
    _cancelScroll(scrollId);
}

// ---------------------------------------------------------------------------
// inView()
// ---------------------------------------------------------------------------

export function motionInView(inViewId, selector, optionsJson, dotnetRef) {
    _stopInView(inViewId);

    const options = _parseOptions(optionsJson);

    // Resolve root CSS selector to a DOM element
    if (options.root && typeof options.root === "string") {
        options.root = document.querySelector(options.root);
    }

    const stop = inView(selector, (_element, _entry) => {
        dotnetRef.invokeMethodAsync("OnEnterView", inViewId);

        // Return a leave callback
        return () => dotnetRef.invokeMethodAsync("OnLeaveView", inViewId);
    }, options);

    inViewStops.set(inViewId, stop);
}

export function motionStopInView(inViewId) {
    _stopInView(inViewId);
}

// ---------------------------------------------------------------------------
// Layout / FLIP
// ---------------------------------------------------------------------------

// Returns {x, y, width, height} of an element's bounding rect. When
// includeTransform is false the current inline transform is temporarily
// cleared so the returned rect reflects the LAYOUT position (useful for
// shared-layout animations where we want the canonical resting rect rather
// than the in-flight visual rect).
export function motionGetRect(selector, includeTransform = true) {
    const el = document.querySelector(selector);
    if (!el) return null;
    if (includeTransform) {
        const r = el.getBoundingClientRect();
        return { x: r.x, y: r.y, width: r.width, height: r.height };
    }
    const savedTransform = el.style.transform;
    el.style.transform = 'none';
    const r = el.getBoundingClientRect();
    el.style.transform = savedTransform;
    return { x: r.x, y: r.y, width: r.width, height: r.height };
}

// ---------------------------------------------------------------------------
// Shared-layout (layoutId-style) transitions
// ---------------------------------------------------------------------------
//
// Design: a single MutationObserver on document.body watches for elements
// carrying [data-sli="<layoutId>"] being added to the DOM or having their
// class attribute changed to include "sli-hidden". When such a mutation is
// observed, the handler runs in the SAME microtask as the mutation — before
// the browser paints. That is the critical property: the animation's
// initial inline transform (displacement back to the element's old rect)
// is applied in the same frame as the DOM change, so the user never sees a
// blank gap between the old element disappearing and the new one appearing.
//
// Contrast with the previous design which waited for Blazor Server's
// OnAfterRenderAsync → SignalR → JS round-trip before calling animate(),
// during which the element was rendered at opacity 0 and invisible for
// tens of milliseconds.
//
// Rect cache: sliRects maps layoutId → last-known viewport rect. Populated
// whenever an element is handled; queried when a new element with the same
// id mounts so we know where to animate FROM. Refreshed on scroll/resize
// so the cached rect stays accurate across user viewport changes.

const sliRects   = new Map();     // layoutId -> { x, y, width, height }
let   sliObserver = null;         // singleton MutationObserver

// One-time initialization of the DOM observer. Idempotent — calling
// multiple times is safe (second+ calls just re-scan for pre-existing
// [data-sli] elements).
export function motionSharedLayoutInit() {
    if (sliObserver) {
        _sliScanExisting();
        return;
    }

    sliObserver = new MutationObserver(mutations => {
        const toHandle = new Set();

        for (const m of mutations) {
            if (m.type === 'childList') {
                // Handle ALL added [data-sli] elements — whether they carry
                // .sli-hidden (fresh Blazor mount) or not (DOM node moved by
                // Blazor's @key reconciler when a keyed list is reordered).
                // Both cases need to animate from the cached previous rect.
                for (const node of m.addedNodes) {
                    if (node.nodeType !== 1) continue;
                    if (node.hasAttribute && node.hasAttribute('data-sli')) {
                        toHandle.add(node);
                    }
                    if (node.querySelectorAll) {
                        node.querySelectorAll('[data-sli]').forEach(e => toHandle.add(e));
                    }
                }
            } else if (m.type === 'attributes' && m.attributeName === 'class') {
                // An existing element's class was patched by Blazor (e.g. shape
                // morph: the size-determining class changed and .sli-hidden was
                // re-added). Only process if .sli-hidden is present — otherwise
                // this is the observer reacting to our own _sliReveal() call.
                const el = m.target;
                if (el.hasAttribute('data-sli') && el.classList.contains('sli-hidden')) {
                    toHandle.add(el);
                }
            }
        }

        toHandle.forEach(_sliHandle);
    });

    sliObserver.observe(document.body, {
        childList:       true,
        subtree:         true,
        attributes:      true,
        attributeFilter: ['class'],
    });

    // Handle any elements that were already in the DOM when the observer started.
    _sliScanExisting();

    // Keep the rect cache fresh across scroll and resize. We only refresh
    // elements that aren't currently animating (no .sli-hidden) because
    // in-flight transforms would corrupt the measurement.
    window.addEventListener('scroll', _scheduleSliRefresh, { passive: true, capture: true });
    window.addEventListener('resize', _scheduleSliRefresh);
}

function _sliScanExisting() {
    // Handle hidden elements (need reveal / animation).
    document.querySelectorAll('[data-sli].sli-hidden').forEach(_sliHandle);
    // Seed rects for already-visible elements so that the first shuffle /
    // reorder has correct "from" positions without needing a prior animation.
    document.querySelectorAll('[data-sli]:not(.sli-hidden)').forEach(el => {
        const id = el.getAttribute('data-sli');
        if (id && !sliRects.has(id)) {
            const r = el.getBoundingClientRect();
            sliRects.set(id, { x: r.x, y: r.y, width: r.width, height: r.height });
        }
    });
}

// Core handler — called for every [data-sli] element that either:
//   • just entered the DOM (fresh Blazor mount, or @key node moved by list
//     reconciliation — may or may not carry .sli-hidden), OR
//   • had its class attribute patched by Blazor to re-add .sli-hidden
//     (same DOM node, new size/position — e.g. the shape-morph demo).
//
// Runs as a microtask (MutationObserver callback) — BEFORE the browser's
// rendering step. Any inline style we write here is visible on the very
// first paint, so the element is never painted at the raw new DOM position.
function _sliHandle(el) {
    const layoutId = el.getAttribute('data-sli');
    if (!layoutId) return;

    // Stop any in-flight animation and clear residual inline transforms so
    // getBoundingClientRect() reflects the true layout position.
    const animId = 'motion_shared_' + layoutId;
    if (animations.has(animId)) {
        try { animations.get(animId).stop(); } catch (_) { /* disposed */ }
        animations.delete(animId);
    }
    el.style.transform       = '';
    el.style.transformOrigin = '';
    el.style.opacity         = '';

    const toRect = el.getBoundingClientRect();
    const prev   = sliRects.get(layoutId);
    sliRects.set(layoutId, { x: toRect.x, y: toRect.y, width: toRect.width, height: toRect.height });

    const opts         = _parseSliOpts(el);
    const includeScale = opts.scale !== false;

    // Always reveal immediately — .sli-hidden is removed here so the element
    // is visible from the very first paint. The synchronous inline transform
    // below positions it at the old rect, so there is no FOUC regardless of
    // whether this is a fresh mount, a DOM move, or a same-element re-render.
    _sliReveal(el);

    if (!prev) {
        // First time this layoutId has been seen — just appear naturally.
        return;
    }

    const dx = prev.x - toRect.x;
    const dy = prev.y - toRect.y;
    let sx = 1, sy = 1;
    if (includeScale) {
        sx = toRect.width  > 0 ? prev.width  / toRect.width  : 1;
        sy = toRect.height > 0 ? prev.height / toRect.height : 1;
    }

    const noPos  = Math.abs(dx) < 0.5 && Math.abs(dy) < 0.5;
    const noSize = Math.abs(sx - 1) < 0.01 && Math.abs(sy - 1) < 0.01;

    if (noPos && noSize) {
        return; // Already revealed; no movement to animate.
    }

    // Set the initial displacement inline so the first paint shows the element
    // at the old rect. WAAPI then takes over from the first rAF onward, using
    // the same starting value (explicit from-keyframe), giving a seamless handoff.
    el.style.transformOrigin = '0 0';
    el.style.transform       = noSize
        ? `translate(${dx}px, ${dy}px)`
        : `translate(${dx}px, ${dy}px) scaleX(${sx}) scaleY(${sy})`;

    const values = { x: [dx, 0], y: [dy, 0] };
    if (!noSize) { values.scaleX = [sx, 1]; values.scaleY = [sy, 1]; }

    const transition = opts.transition || {};
    const anim       = animate(el, values, transition);
    animations.set(animId, anim);

    anim.then(() => {
        el.style.transform       = '';
        el.style.transformOrigin = '';
        // Refresh the cached rect with the final resting position.
        const r = el.getBoundingClientRect();
        sliRects.set(layoutId, { x: r.x, y: r.y, width: r.width, height: r.height });
    }).catch(() => { /* cancelled — next invocation will clean up */ });
}

function _sliReveal(el) {
    el.classList.remove('sli-hidden');
    el.style.opacity = '';
}

function _parseSliOpts(el) {
    const raw = el.getAttribute('data-sli-opts');
    if (!raw) return {};
    try { return JSON.parse(raw); } catch { return {}; }
}

// Throttle scroll/resize rect refreshes to one per animation frame.
let sliRefreshScheduled = false;
function _scheduleSliRefresh() {
    if (sliRefreshScheduled) return;
    sliRefreshScheduled = true;
    requestAnimationFrame(() => {
        sliRefreshScheduled = false;
        document.querySelectorAll('[data-sli]').forEach(el => {
            // Skip elements currently animating — their getBoundingClientRect
            // reflects the in-flight transform, not the layout position.
            if (el.classList.contains('sli-hidden')) return;
            const id = el.getAttribute('data-sli');
            if (!id) return;
            const r = el.getBoundingClientRect();
            sliRects.set(id, { x: r.x, y: r.y, width: r.width, height: r.height });
        });
    });
}

// FLIP-animate: after a layout change, animate element from its old position
// (fromX, fromY) to its new DOM position. Measures the new position WITHOUT
// the element's current transform so in-flight FLIPs don't corrupt the delta.
export function motionFlipFromRect(animId, selector, fromX, fromY, optionsJson) {
    const el = document.querySelector(selector);
    if (!el) return;

    // Stop (don't cancel — cancel would revert to the pre-animation state)
    // any in-flight animation with the same animId so we can measure cleanly.
    if (animations.has(animId)) {
        try { animations.get(animId).stop(); } catch (_) { /* disposed */ }
        animations.delete(animId);
    }

    // Measure the element's LAYOUT position by temporarily clearing any
    // transform. This is synchronous within the same frame so no repaint
    // happens between the clear and the restore.
    const savedTransform = el.style.transform;
    el.style.transform = 'none';
    const layoutRect = el.getBoundingClientRect();
    el.style.transform = savedTransform;

    const dx = fromX - layoutRect.x;
    const dy = fromY - layoutRect.y;

    if (Math.abs(dx) < 0.5 && Math.abs(dy) < 0.5) {
        // No perceptible movement — clear any residual transform left over
        // from a previous FLIP that was interrupted.
        el.style.transform = '';
        return;
    }

    const options = _parseOptions(optionsJson);
    const anim = animate(el, { x: [dx, 0], y: [dy, 0] }, options);
    animations.set(animId, anim);
}

// Batched FLIP: measures layout positions for every target in one pass
// (avoiding layout thrashing), then animates each one. Takes precedence
// over calling motionFlipFromRect in a loop — it eliminates N round-trips
// from .NET → JS and keeps all the measurements on the same frame.
export function motionFlipMany(flipsJson, optionsJson) {
    const flips = JSON.parse(flipsJson);

    // Phase 1 — stop any in-flight animations for these animIds, then
    // capture each element and its saved transform.
    const entries = flips.map(f => {
        const el = document.querySelector(f.selector);
        if (!el) return null;

        if (animations.has(f.animId)) {
            try { animations.get(f.animId).stop(); } catch (_) { /* disposed */ }
            animations.delete(f.animId);
        }

        return { el, f, savedTransform: el.style.transform };
    });

    // Phase 2 — clear all transforms (batched style writes).
    for (const e of entries) {
        if (e) e.el.style.transform = 'none';
    }

    // Phase 3 — measure all layout rects (batched layout read).
    for (const e of entries) {
        if (e) e.layoutRect = e.el.getBoundingClientRect();
    }

    // Phase 4 — restore transforms (batched style writes).
    for (const e of entries) {
        if (e) e.el.style.transform = e.savedTransform;
    }

    // Phase 5 — compute deltas and start each animation.
    const options = _parseOptions(optionsJson);
    for (const e of entries) {
        if (!e) continue;

        const dx = e.f.fromX - e.layoutRect.x;
        const dy = e.f.fromY - e.layoutRect.y;

        if (Math.abs(dx) < 0.5 && Math.abs(dy) < 0.5) {
            // No movement — clear any lingering transform.
            e.el.style.transform = '';
            continue;
        }

        const anim = animate(e.el, { x: [dx, 0], y: [dy, 0] }, options);
        animations.set(e.f.animId, anim);
    }
}

// ---------------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------------

function _parseOptions(optionsJson) {
    if (!optionsJson) return {};
    const options = JSON.parse(optionsJson);

    // Convert sentinel values
    if (options._repeatInfinite) {
        options.repeat = Infinity;
        delete options._repeatInfinite;
    }

    return options;
}

function _cancelAnimation(animId) {
    if (animations.has(animId)) {
        try { animations.get(animId).cancel(); } catch (_) { /* disposed */ }
        animations.delete(animId);
    }
}

function _cancelScroll(scrollId) {
    if (scrollCancels.has(scrollId)) {
        try { scrollCancels.get(scrollId)(); } catch (_) { /* disposed */ }
        scrollCancels.delete(scrollId);
    }
}

function _stopInView(inViewId) {
    if (inViewStops.has(inViewId)) {
        try { inViewStops.get(inViewId)(); } catch (_) { /* disposed */ }
        inViewStops.delete(inViewId);
    }
}

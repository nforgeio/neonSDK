using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;


namespace Neon.Blazor
{
    /// <summary>
    /// Enumerates swipe directions.
    /// </summary>
    public enum SwipeDirection
    {
        None,
        //
        // Summary:
        //     Indicates a rightward swipe.
        Right,
        //
        // Summary:
        //     Indicates a leftward swipe.
        Left,
        //
        // Summary:
        //     Indicates an upward swipe.
        Up,
        //
        // Summary:
        //     Indicates a downward swipe.
        Down
    }
}

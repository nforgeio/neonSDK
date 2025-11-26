// -----------------------------------------------------------------------------
// FILE:	    PolyLineHelper.cs
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
    public static class PolyLineHelper
    {
        public const string BaseUrl = "https://api.mapbox.com";

        public static string GetMapUrl(string polyLine, int width, int height)
        {
            if (polyLine == null)
            {
                return null;
            }

            return $"{BaseUrl}/styles/v1/mapbox/streets-v11/static/path-8+ff0000({polyLine})/auto/{width}x{height}?access_token={MapboxMap.globalAccessToken}";
        }
    }
}
